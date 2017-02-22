﻿using Mapsui.Forms.Extensions;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using System;
using System.ComponentModel;
using Xamarin.Forms.Maps;

namespace Mapsui.Forms
{
	/// <summary>
	/// This is the Mapsui Forms Control that will be used within the Forms PCL project
	/// </summary>
	public class MapView : View, INotifyPropertyChanged
	{
		/// <summary>
		/// Privates
		/// </summary>
		internal Map nativeMap;

		public MapView() : this(new MapSpan(new Position(41.890202, 12.492049), 0.1, 0.1))
		{
		}

		public MapView(MapSpan startPosition = null)
		{
			Map = new Map();

			if (startPosition != null)
			{
				VisibleRegion = startPosition;
			}
		}

		/// <summary>
		/// Events
		/// </summary>

		public new PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Properties
		/// </summary>

		public Map Map
		{
			get
			{
				return nativeMap;
			}
			set
			{
				if (nativeMap == value)
					return;

				nativeMap = value;
				// Replace Viewport with NotifyViewport, so that we get events when Viewport changes
				var oldViewport = nativeMap.Viewport as Viewport;
				if (oldViewport != null)
				{
					var newViewport = new NotifyingViewport(oldViewport);
					newViewport.PropertyChanged += ViewportPropertyChanged;
					nativeMap.Viewport = newViewport;
				}
				RefreshGraphics();
				// Get values
				//Center = nativeMap.Viewport.Center;
				// Set values
				VisibleRegion = LastMoveToRegion;
				nativeMap.BackColor = BackgroundColor.ToMapsuiColor();
			}
		}

		internal MapSpan LastMoveToRegion { get; private set; }

		public MapSpan VisibleRegion
		{
			get {
				if (nativeMap == null)
					return null;

				var leftBottom = Projection.SphericalMercator.ToLonLat(nativeMap.Viewport.Extent.BottomLeft.X, nativeMap.Viewport.Extent.BottomLeft.X);
				var rightTop = Projection.SphericalMercator.ToLonLat(nativeMap.Viewport.Extent.TopRight.X, nativeMap.Viewport.Extent.TopRight.X);
				var center = Projection.SphericalMercator.ToLonLat(nativeMap.Viewport.Center.X, nativeMap.Viewport.Center.Y);

				return new MapSpan(new Position(center.Y, center.X), Math.Abs(rightTop.Y - leftBottom.Y) / 2, Math.Abs(leftBottom.X - rightTop.X) / 2);
			}
			set
			{
				UpdateVisibleRegion(value);
			}
		}

		public Position Center
		{
			get { return (Position)GetValue(CenterProperty); }
			set { SetValue(CenterProperty, value); }
		}

		/// <summary>
		/// Bindings
		/// </summary>
		 
		public static readonly BindableProperty CenterProperty = BindableProperty.Create(
										propertyName: nameof(Center),
										returnType: typeof(Position),
										declaringType: typeof(MapView),
										defaultValue: default(Position),
										defaultBindingMode: BindingMode.TwoWay,
										propertyChanged: null);

		/// <summary>
		/// Methods
		/// </summary>
		
		/// Change Viewport 
		public void MoveToRegion(MapSpan mapSpan)
		{
			if (mapSpan == null)
				throw new ArgumentNullException(nameof(mapSpan));
			LastMoveToRegion = mapSpan;
			VisibleRegion = mapSpan;
		}

		/// <summary>
		/// Refresh the graphics of the map
		/// </summary>
		public void RefreshGraphics()
		{
			MessagingCenter.Send<MapView>(this, "Refresh");
		}

		/// <summary>
		/// Check if something important for Map changed
		/// </summary>
		/// <param name="propertyName">Name of property which changed</param>
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			// Set new BackgroundColor to nativeMap
			if (propertyName.Equals(nameof(BackgroundColor)))
			{
				nativeMap.BackColor = BackgroundColor.ToMapsuiColor();
			}

			if (propertyName.Equals(nameof(Center)))
			{
				VisibleRegion = new MapSpan(Center, LastMoveToRegion.LatitudeDegrees, LastMoveToRegion.LongitudeDegrees);
			}

			RaisePropertyChanged(propertyName);
		}

		/// <summary>
		/// Get updates from Map
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void MapPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			RaisePropertyChanged(e.PropertyName);
		}

		/// <summary>
		/// Get updates from Viewport
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void ViewportPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if ((e.PropertyName == nameof(IViewport.Width) | e.PropertyName == nameof(IViewport.Height))
				&& nativeMap.Viewport.Width != 0 && nativeMap.Viewport.Height != 0)
			{
				UpdateVisibleRegion(LastMoveToRegion);
			}

			RaisePropertyChanged(e.PropertyName);
		}

		/// <summary>
		/// Raise event for PropertyChanged of MapView
		/// </summary>
		/// <param name="propertyName"></param>
		void RaisePropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		void UpdateVisibleRegion(MapSpan newMapSpan)
		{
			if (newMapSpan == null || VisibleRegion.Equals(newMapSpan) || nativeMap == null)
				return;

			LastMoveToRegion = newMapSpan;

			var top = newMapSpan.Center.Latitude + newMapSpan.LatitudeDegrees;
			var latBottom = newMapSpan.Center.Latitude - newMapSpan.LatitudeDegrees;
			var lonLeft = newMapSpan.Center.Longitude - newMapSpan.LongitudeDegrees;
			var lonRight = newMapSpan.Center.Longitude + newMapSpan.LongitudeDegrees;

			var leftBottom = Projection.SphericalMercator.FromLonLat(lonLeft, latBottom);
			var rightTop = Projection.SphericalMercator.FromLonLat(lonRight, top);

			nativeMap.NavigateTo(new Geometries.BoundingBox(leftBottom, rightTop));
		}
	}
}