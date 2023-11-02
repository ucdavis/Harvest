import React, { useEffect, useRef } from "react";
import WebMap from "@arcgis/core/WebMap";
import Bookmarks from "@arcgis/core/widgets/Bookmarks";
import Expand from "@arcgis/core/widgets/Expand";
import MapView from "@arcgis/core/views/MapView";
import Editor from "@arcgis/core/widgets/Editor";
import FeatureLayer from "@arcgis/core/layers/FeatureLayer";
import BasemapToggle from "@arcgis/core/widgets/BasemapToggle";

// from: https://github.com/Esri/jsapi-resources/blob/main/esm-samples/jsapi-react/src/App.jsx
export const Map = () => {
  console.log("Map.tsx");
  const mapDiv = useRef(null);

  useEffect(() => {
    if (mapDiv.current) {
      /**
       * Initialize application
       */
      // https://developers.arcgis.com/javascript/latest/api-reference/esri-WebMap.html
      const webmap = new WebMap({
        portalItem: {
          // the unique id for your map
          // you can access this by going to arcgis -> content -> my content -> map and copy the id on the right (or from url)
          id: "8e276b0a2b6b49b88a1ea459d91f5fd3",
        },
        basemap: "hybrid", // https://developers.arcgis.com/rest/basemap-styles/#arcgis-styles
      });

      const view = new MapView({
        container: mapDiv.current,
        map: webmap,
        center: [-121.748, 38.54],
        zoom: 13,
      });

      // const bookmarks = new Bookmarks({
      //   view,
      //   // allows bookmarks to be added, edited, or deleted
      //   editingEnabled: true,
      // });

      // const bkExpand = new Expand({
      //   view,
      //   content: bookmarks,
      //   expanded: true,
      // });
      // Add the widget to the top-right corner of the view
      // view.ui.add(bkExpand, "top-right");

      const featureLayer = new FeatureLayer({
        portalItem: {
          id: "66289d4facfb4932a5b1d91db8792c4f",
        },
      });
      webmap.add(featureLayer);

      // Begin Editor constructor
      const editor = new Editor({
        view: view,
      }); // End Editor constructor

      // Add the widget to the view
      view.ui.add(editor, "top-right");

      // https://developers.arcgis.com/javascript/latest/api-reference/esri-widgets-BasemapToggle.html
      let basemapToggle = new BasemapToggle({
        view: view, // The view that provides access to the map's "streets-vector" basemap
        nextBasemap: "osm", // Allows for toggling to the "osm" basemap
      });
      view.ui.add(basemapToggle, "top-left");

      // bonus - how many bookmarks in the webmap?
      webmap.when(() => {
        if (webmap.bookmarks && webmap.bookmarks.length) {
          console.log("Bookmarks: ", webmap.bookmarks.length);
        } else {
          console.log("No bookmarks in this webmap.");
        }
      });
    }
  }, [mapDiv]);

  return <div className="mapDiv" ref={mapDiv}></div>;
};
