using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kogane.Internal
{
    internal static class ConvertTextureSizeMultipleOf4MenuItem
    {
        private const string MENU_ITEM_NAME = "Assets/Kogane/Convert Texture Size Multiple Of 4";

        [MenuItem( MENU_ITEM_NAME, true )]
        private static bool CanConvert()
        {
            return Selection.objects.OfType<Texture2D>().Any();
        }

        [MenuItem( MENU_ITEM_NAME, false, 1155415621 )]
        private static void Convert()
        {
            if ( !EditorUtility.DisplayDialog( "", "選択中のすべてのテクスチャのサイズを 4 の倍数に変換しますか？", "はい", "いいえ" ) ) return;

            var textureArray = Selection.objects.OfType<Texture2D>().ToArray();

            try
            {
                AssetDatabase.StartAssetEditing();

                var length = textureArray.Length;

                for ( var i = 0; i < length; i++ )
                {
                    var number     = i + 1;
                    var oldTexture = textureArray[ i ];

                    EditorUtility.DisplayProgressBar
                    (
                        title: "Convert Texture Size Multiple Of 4",
                        info: $"{number} / {length}    {oldTexture.name}",
                        progress: ( float )number / length
                    );

                    var oldWidth  = oldTexture.width;
                    var oldHeight = oldTexture.height;
                    var newWidth  = ( int )RoundByMultiple( oldWidth, 4 );
                    var newHeight = ( int )RoundByMultiple( oldHeight, 4 );

                    if ( oldWidth == newWidth && oldHeight == newHeight ) continue;

                    var newTexture = ResizeTexture( oldTexture, newWidth, newHeight );
                    var assetPath  = AssetDatabase.GetAssetPath( oldTexture );
                    var bytes      = newTexture.EncodeToPNG();

                    File.WriteAllBytes( assetPath, bytes );
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog( "", "選択中のすべてのテクスチャのサイズを 4 の倍数に変換しました", "OK" );
        }

        private static double RoundByMultiple( double value, double unit )
        {
            return Math.Round( value / unit ) * unit;
        }

        private static Texture2D ResizeTexture( Texture texture, int width, int height )
        {
            var temporaryRenderTexture = RenderTexture.GetTemporary( width, height );

            Graphics.Blit( texture, temporaryRenderTexture );

            var oldRenderTexture = RenderTexture.active;

            RenderTexture.active = temporaryRenderTexture;

            var newTexture = new Texture2D( width, height );
            newTexture.ReadPixels( new( 0, 0, width, height ), 0, 0 );
            newTexture.Apply();

            RenderTexture.active = oldRenderTexture;
            RenderTexture.ReleaseTemporary( temporaryRenderTexture );

            return newTexture;
        }
    }
}