﻿using Mapsui.Geometries;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    internal static class PolygonRenderer
    {
        public static void Draw(SKCanvas canvas, IViewport viewport, IStyle style, IGeometry geometry)
        {
            var polygon = (Polygon)geometry;
            
            float lineWidth = 1;
            var lineColor = Color.Black; // default
            var fillColor = Color.Gray; // default

            var vectorStyle = style as VectorStyle;

            if (vectorStyle != null)
            {
                lineWidth = (float) vectorStyle.Outline.Width;
                lineColor = vectorStyle.Outline.Color;
                fillColor = vectorStyle.Fill.Color;
            }

            using (var path = ToSkia(viewport, polygon))
            {
                using (var paint = new SKPaint())
                {
                    paint.IsAntialias = true;
                    paint.StrokeWidth = lineWidth;

                    paint.Style = SKPaintStyle.Fill;
                    paint.Color = fillColor.ToSkia();
                    canvas.DrawPath(path, paint);
                    paint.Style = SKPaintStyle.Stroke;
                    paint.Color = lineColor.ToSkia();
                    canvas.DrawPath(path, paint);
                }
            }
        }

        private static SKPath ToSkia(IViewport viewport, Polygon polygon)
        {
            var vertices = polygon.ExteriorRing.Vertices;
            var path = new SKPath();
            {
                // todo: use transform matrix
                var first = viewport.WorldToScreen(vertices[0].X, vertices[0].Y);
                path.MoveTo((float)first.X, (float) first.Y);

                for (var i = 1; i < vertices.Count; i++)
                {
                   var point = viewport.WorldToScreen(vertices[i].X, vertices[i].Y);
                    path.LineTo((float) point.X, (float) point.Y);
                }
                path.Close();
                foreach (var interiorRing in polygon.InteriorRings)
                {
                    // note: For Skia inner rings need to be clockwise and outer rings
                    // need to be counter clockwise (if this is the other way around it also
                    // seems to work)
                    // this is not a requirement of the OGC polygon.
                    interiorRing.Vertices.Reverse();
                    var firstInner = viewport.WorldToScreen(interiorRing.Vertices[0].X, interiorRing.Vertices[0].Y);
                    path.MoveTo((float)firstInner.X, (float)firstInner.Y);
                    for (var i = 1; i < interiorRing.Vertices.Count; i++)
                    {
                        var point = viewport.WorldToScreen(interiorRing.Vertices[i].X, interiorRing.Vertices[i].Y);
                        path.LineTo((float)point.X, (float)point.Y);
                    }
                }
                path.Close();
                return path;
            }
        }
    }
}