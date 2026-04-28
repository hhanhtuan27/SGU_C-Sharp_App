#if ANDROID
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Microsoft.Maui.Maps.Handlers;

namespace VinhKhanhMaui.Platforms.Android;

/// <summary>
/// Ẩn tất cả POI rác Google Maps (trạm xăng, siêu thị...), chỉ giữ
/// đường sá + marker của app. Dùng JSON style.
/// </summary>
public class CustomMapHandler : MapHandler
{
    // JSON style inline (không cần file riêng, tránh lỗi resource)
    private const string MapStyleJson = @"[
        {""featureType"":""poi"",""stylers"":[{""visibility"":""off""}]},
        {""featureType"":""poi.park"",""stylers"":[{""visibility"":""on""}]},
        {""featureType"":""transit"",""stylers"":[{""visibility"":""off""}]}
    ]";

    protected override void ConnectHandler(global::Android.Gms.Maps.MapView platformView)
    {
        base.ConnectHandler(platformView);

        try
        {
            platformView.GetMapAsync(new MapReadyCallback());
        }
        catch
        {
            // Nếu lỗi → map hiện bình thường, không crash
        }
    }

    private class MapReadyCallback : Java.Lang.Object, IOnMapReadyCallback
    {
        public void OnMapReady(GoogleMap googleMap)
        {
            try
            {
                var style = new MapStyleOptions(MapStyleJson);
                googleMap.SetMapStyle(style);
            }
            catch
            {
                // Fallback: map hiện bình thường
            }
        }
    }
}
#endif