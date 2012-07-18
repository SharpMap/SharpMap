$(document).ready(function () {
    var map = new L.Map('map'), zoom = 16, cloudmade;
    cloudmade = new L.TileLayer(['http://{s}tile.cloudmade.com', '/1a235b638b614b458deeb77c7dae4f80', '/998/256/{z}/{x}/{y}.png'].join(''), {
        maxZoom: 18,
        subdomains: ['a.', 'b.', 'c.', '']
    });
    map.addLayer(cloudmade);

    $('#link1').click(function () {
        map.setView(new L.LatLng(52.52111, 13.40988), zoom);
    }).click();
    $('#link2').click(function () {
        map.setView(new L.LatLng(52.50440, 13.33522), zoom);
    });
    $('#link3').click(function () {
        map.setView(new L.LatLng(52.50983, 13.37455), zoom);
    });
    
    Buildings.setMap('leaflet', map);
    Buildings.load("/buildings/getdata?w={w}&n={n}&e={e}&s={s}&z={z}&all=true", {
        strokeRoofs: false,
        wallColor: "rgb(190,170,150)",
        roofColor: "rgb(230,220,210)",
        strokeColor: "rgb(145,140,135)"
    });
});