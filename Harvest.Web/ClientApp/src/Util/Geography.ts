import * as GeoJSON from "geojson";

interface Bounds {
  xMin: number;
  xMax: number;
  yMin: number;
  yMax: number;
}
export const getBoundingBox = (polygons: GeoJSON.Polygon[]) => {
  const bounds: Bounds = {} as Bounds;

  for (let i = 0; i < polygons.length; i++) {
    const coordinates = polygons[i].coordinates;

    if (coordinates.length === 1) {
      // It's only a single Polygon
      // For each individual coordinate in this feature's coordinates...
      for (let j = 0; j < coordinates[0].length; j++) {
        let longitude = coordinates[0][j][0];
        let latitude = coordinates[0][j][1];

        // Update the bounds by comparing the current xMin/xMax and yMin/yMax with the current coordinate
        bounds.xMin = bounds.xMin < longitude ? bounds.xMin : longitude;
        bounds.xMax = bounds.xMax > longitude ? bounds.xMax : longitude;
        bounds.yMin = bounds.yMin < latitude ? bounds.yMin : latitude;
        bounds.yMax = bounds.yMax > latitude ? bounds.yMax : latitude;
      }
    } else {
      console.log("multi-poly is unsupported");
    }
  }

  // Returns an object that contains the bounds of this GeoJSON data.
  // The keys describe a box formed by the northwest (xMin, yMin) and southeast (xMax, yMax) coordinates.
  return bounds;
};
