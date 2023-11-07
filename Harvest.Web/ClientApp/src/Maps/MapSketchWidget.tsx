import React, { useEffect, useRef, useState } from "react";
import WebMap from "@arcgis/core/WebMap";
import MapView from "@arcgis/core/views/MapView";
import FeatureLayer from "@arcgis/core/layers/FeatureLayer";
import BasemapToggle from "@arcgis/core/widgets/BasemapToggle";
import Sketch from "@arcgis/core/widgets/Sketch";
import GraphicsLayer from "@arcgis/core/layers/GraphicsLayer";
import Graphic from "@arcgis/core/Graphic.js";
import Collection from "@arcgis/core/core/Collection.js";
import SketchViewModel from "@arcgis/core/widgets/Sketch/SketchViewModel.js";
import * as reactiveUtils from "@arcgis/core/core/reactiveUtils.js";

// from: https://github.com/Esri/jsapi-resources/blob/main/esm-samples/jsapi-react/src/App.jsx
export const MapSketchWidget = () => {
  console.log("Map.tsx");
  const mapDiv = useRef(null);

  const [graphics, setGraphics] = useState<Collection<Graphic>>(
    new Collection()
  );

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

      const featureLayer = new FeatureLayer({
        portalItem: {
          id: "66289d4facfb4932a5b1d91db8792c4f",
        },
      });
      webmap.add(featureLayer);

      // https://developers.arcgis.com/javascript/latest/api-reference/esri-widgets-BasemapToggle.html
      let basemapToggle = new BasemapToggle({
        view: view, // The view that provides access to the map's "streets-vector" basemap
        nextBasemap: "osm", // Allows for toggling to the "osm" basemap
      });
      view.ui.add(basemapToggle, "top-left");

      const graphicsLayer = new GraphicsLayer();
      const sketch = new Sketch({
        layer: graphicsLayer,
        view: view,
        // graphic will be selected as soon as it is created
        creationMode: "update",
      });
      view.ui.add(sketch, "top-right");

      // const handle = reactiveUtils.watch(
      //   // getValue function
      //   () => graphicsLayer.graphics,
      //   // Callback function
      //   (newValue, oldValue) => {
      //     console.log("New value: ", newValue, "Old value: ", oldValue);
      //   },
      //   // Optional parameters
      //   {
      //     // initial: true,
      //   }
      // );
    }
  }, [mapDiv]);

  return <div className="mapDiv" ref={mapDiv}></div>;
};
