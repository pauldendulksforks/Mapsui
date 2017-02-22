﻿using Mapsui.Projection;
using Mapsui.Samples.Common.Maps;
using System;
using Xamarin.Forms;

namespace Mapsui.Samples.Forms
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();

			mapView.Map = BingSample.CreateMap();

//			mapView.Map = InfoLayersSample.CreateMap();
//			mapView.Map = LabelsSample.CreateMap();
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			// Get the lon lat coordinates from somewhere (Mapsui can not help you there)
			var centerOfLondonOntario = new Point(-81.2497, 42.9837);
			// OSM uses spherical mercator coordinates. So transform the lon lat coordinates to spherical mercator
			var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(centerOfLondonOntario.X, centerOfLondonOntario.Y);
			// Set the center of the viewport to the coordinate. The UI will refresh automatically
			mapView.Map.Viewport.Center = sphericalMercatorCoordinate;
			// Additionally you might want to set the resolution, this could depend on your specific purpose
			mapView.Map.Viewport.Resolution = mapView.Map.Resolutions[9];
			((Button)sender).Text = mapView?.Center?.ToString(); 
		}
	}
}