$(document).ready(function () {
    var map = new L.Map('map'), zoom = 16;
    var osm = new L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    })
    map.addLayer(osm);
    
    $('#link1').click(function () {
        map.setView(new L.LatLng(52.52111, 13.40988), zoom);
    }).click();
    $('#link2').click(function () {
        map.setView(new L.LatLng(52.50440, 13.33522), zoom);
    });
    $('#link3').click(function () {
        map.setView(new L.LatLng(52.50983, 13.37455), zoom);
    });

    var buildings = new OSMBuildings(map);
    buildings.loadData('/buildings/getdata?w={w}&n={n}&e={e}&s={s}&z={z}');    
});
