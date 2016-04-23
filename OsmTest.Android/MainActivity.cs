﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using GeoJSON.Net.Geometry;
using OsmDroid;
using OsmTest.Android.Model;
using OsmTest.Android.Services;
using Environment = Android.OS.Environment;

using OsmDroid.Api;
using OsmDroid.TileProvider.TileSource;
using OsmDroid.Util;
using OsmDroid.Views;
using OsmDroid.Views.Overlay;
using OsmSharp.Geo.Geometries;
using OsmSharp.UI.Map.Styles;
using OsmTest.Core.Services;


namespace OsmTest.Android
{
	[Activity (Label = "OsmTest", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@style/Theme.AppCompat")]
	public class MainActivity : ActionBarActivity
   {
      /// <summary>
      /// Holds the mapview.
      /// </summary>
      private MapView _mapView;

	   ApiService _service = null;


      private CancellationTokenSource _cancellationTokenSource;
	   private string css = @"
node
{
    color:#f00;
    width:2;
    opacity:0.7;
    fill-color:#fc0;
    fill-opacity:0.3;
}
way
{
    color:#03f;
    width:5;
    opacity:0.6;
}
area
{
    color:#03f;
    width:2;
    opacity:0.7;
    fill-color:#fc0;
    fill-opacity:0.3;
}
relation node, relation way, relation relation
{
    color:#d0f;
}";
      private IMapController _mapController;
      //private MapView _mapView;


      protected async override void OnCreate(Bundle bundle)
      {
         base.OnCreate(bundle);
         SetContentView(Resource.Layout.Main);
         _service = new ApiService();
         StyleInterpreter interpreter = null;
         try
         {
            bool isTile = true;
            //List<OsmGeo> osm = null;
            if (isTile)
            {
               _mapView = FindViewById<MapView>(Resource.Id.mapview);
               _mapView.SetTileSource(TileSourceFactory.DefaultTileSource);
               _mapView.SetBuiltInZoomControls(true);

               //List<OverlayItem> overlayItemArray = new List<OverlayItem>();
               //OverlayItem olItem = new OverlayItem("Here", "SampleDescription", new GeoPoint(34.878039, -10.650));
               //overlayItemArray.Add(olItem);
               //olItem.SetMarker(Resources.GetDrawable(Resource.Drawable.cloud));
               //overlayItemArray.Add(new OverlayItem("Hi", "You're here", new GeoPoint(34.888039, -10.660)));


               DefaultResourceProxyImpl defaultResourceProxyImpl = new DefaultResourceProxyImpl(this);
               //ItemizedIconOverlay myItemizedIconOverlay = new ItemizedIconOverlay(overlayItemArray, null, defaultResourceProxyImpl);
               //_mapView.Overlays.Add(myItemizedIconOverlay);

               //PathOverlay myPath = new PathOverlay(Color.Red, this);
               //myPath.AddPoint(new GeoPoint(34.878039, -10.650));
               //myPath.AddPoint(new GeoPoint(34.888039, -10.660));
               //_mapView.Overlays.Add(myPath);

               _mapController = _mapView.Controller;
               _mapController.SetZoom(25);
               
               var centreOfMap = new GeoPoint(-6.3423888, 30.392372);
               //var centreOfMap = new GeoPoint(34878039, -104650);
               _mapController.SetCenter(centreOfMap);


               IGeoObjectsService service = new CouchDbGeoObjectsService();
               var points = service.GetCloseUsers(null, 0);
               var firstPoint = ((GeoJSON.Net.Geometry.Point) points.Features[0].Geometry);
               double x = ((GeographicPosition) firstPoint.Coordinates).Latitude;
               double y = ((GeographicPosition)firstPoint.Coordinates).Longitude;
               List<OverlayItem> overlayItemArray = new List<OverlayItem>();
               OverlayItem olItem = new OverlayItem("Here", "SampleDescription", new GeoPoint(x, y));
               overlayItemArray.Add(olItem);
               olItem.SetMarker(Resources.GetDrawable(Resource.Drawable.cloud));
               ItemizedIconOverlay newPoints = new ItemizedIconOverlay(overlayItemArray, null, defaultResourceProxyImpl);
               _mapView.Overlays.Add(newPoints);
            }
            else
            {
               /*List<OsmGeo> osm = await _service.DownloadArea(34.878039, -10.465, 36, -9.077);
               if (osm != null)
               {
                  Native.Initialize();

                  // initialize map.
                  var map = new Map();
                  interpreter = new MapCSSInterpreter(css);

                  IDataSourceReadOnly source = new MemoryDataSource(osm.ToArray());
                  var layer = map.AddLayerOsm(source, interpreter);

                  _mapView = new MapView(this, new MapViewSurface(this));
                  _mapView.Map = map;
                  _mapView.MapMaxZoomLevel = 17; // limit min/max zoom because MBTiles sample only contains a small portion of a map.
                  _mapView.MapMinZoomLevel = 1;
                  _mapView.MapTilt = 0;
                  _mapView.MapCenter = new GeoCoordinate(-9.2, 36);
                  _mapView.MapZoom = 2;
                  _mapView.MapAllowTilt = false;

                  //OsmSharp.Data.SQLite.SQLiteConnectionBase sqLiteConnection = new SQLiteConnection("osmMap");
                 
                  frame.AddView(_mapView);
                  var textLabel = FindViewById<TextView>(Resource.Id.text_label);
                  textLabel.Text = "Изображение подгрузилось";
                  Toast.MakeText(this, "Read success", ToastLength.Long).Show();
               }
               else
               {
                  Toast.MakeText(this, "OSM is null", ToastLength.Long).Show();
               }*/
            }
            
         }
         catch (Exception exception)
         {
            Toast.MakeText(this, exception.Message, ToastLength.Long).Show();
            SetContentView(Resource.Layout.Main);
         }
      }

	   protected async override void OnResume()
	   {
	      base.OnResume();
	   }

	   protected override void OnPause()
	   {
	      base.OnPause();
      }

	   public override bool OnCreateOptionsMenu(IMenu menu)
      {
         var inflater = MenuInflater;
         inflater.Inflate(Resource.Menu.action_items, menu);
         return true;
      }

      public override bool OnOptionsItemSelected(IMenuItem item)
      {
         switch (item.ItemId)
         {
            case Resource.Id.atn_direct_enable:
               UpdateFromService();
               return true;
            case Resource.Id.atn_direct_discover:
               return true;
            default:
               return base.OnOptionsItemSelected(item);
         }
      }

	   private async void UpdateFromService()
	   {
         ApiService service = new ApiService();

         string testData = await service.GetTestData();
         Toast toast = Toast.MakeText(this, testData, ToastLength.Long);
         toast.Show();
      }

	   //private async Task<MvxGeoLocation> UpdateLocation()
	   //{
	   //   _cancellationTokenSource?.Cancel();
	   //   _cancellationTokenSource = new CancellationTokenSource();
    //     var locationService = Mvx.Resolve<LocationService>();
	   //   return await locationService.GetLocation(_cancellationTokenSource);
	   //}
   }
}

