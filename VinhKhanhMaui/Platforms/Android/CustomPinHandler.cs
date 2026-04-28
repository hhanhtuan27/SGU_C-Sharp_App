#if ANDROID
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Microsoft.Maui.Maps.Handlers;

namespace VinhKhanhMaui.Platforms.Android;

/// <summary>
/// Đổi màu pin theo category bằng cách hook vào MapHandler.
/// Ốc=đỏ, Nướng=cam, Lẩu=vàng, Cà phê=xanh, Khác=tím.
/// </summary>
public class ColoredMapHandler : MapHandler
{
    protected override void ConnectHandler(global::Android.Gms.Maps.MapView platformView)
    {
        base.ConnectHandler(platformView);

        try
        {
            platformView.GetMapAsync(new ColoredMapCallback());
        }
        catch { }
    }

    private class ColoredMapCallback : Java.Lang.Object, IOnMapReadyCallback
    {
        public void OnMapReady(GoogleMap googleMap)
        {
            try
            {
                // Ẩn icon rác Google Maps
                var style = new MapStyleOptions(@"[
                    {""featureType"":""poi"",""stylers"":[{""visibility"":""off""}]},
                    {""featureType"":""poi.park"",""stylers"":[{""visibility"":""on""}]},
                    {""featureType"":""transit"",""stylers"":[{""visibility"":""off""}]}
                ]");
                googleMap.SetMapStyle(style);
            }
            catch { }
        }
    }
}
#endif