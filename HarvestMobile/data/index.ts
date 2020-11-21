import { MeasurementUnit, ActionType, Action, TimeSheet, Location, Project } from "../types";

export const measurementUnits: MeasurementUnit[] = [
  { id: 0, name: "Acre" },
  { id: 1, name: "Hour" },
  { id: 2, name: "Sample" },
  { id: 3, name: "Day" },
  { id: 4, name: "Gallon" },
  { id: 5, name: "Each" },
  { id: 6, name: "Event" },
  { id: 7, name: "Half-Day" }
];

export const actionTypes: ActionType[] = [
  { id: 0, name: "Labor" },
  { id: 1, name: "Other" },
  { id: 2, name: "Equipment" },
];

export const actions: Action[] = [
  { id: 0, type: actionTypes[2], name: "PLS Row Planter", unit: measurementUnits[0] },
  { id: 1, type: actionTypes[2], name: "PLS Cab Tractor", unit: measurementUnits[1] },
  { id: 2, type: actionTypes[2], name: "PLS 15 Row Tractor", unit: measurementUnits[1] },
  { id: 3, type: actionTypes[1], name: "Crop Destruction", unit: measurementUnits[0] },
  { id: 4, type: actionTypes[1], name: "Century Project Soil Sample", unit: measurementUnits[2] },
  { id: 5, type: actionTypes[0], name: "RR Mechanic", unit: measurementUnits[1] },
  { id: 6, type: actionTypes[0], name: "Century Project Skilled Labor", unit: measurementUnits[1] },
  { id: 7, type: actionTypes[0], name: "PLS Farm Labor", unit: measurementUnits[1] },
];

export const locations: Location[] = [
  { id: 0, name: "E-1A", unit: measurementUnits[0], size: 1 },
  { id: 1, name: "E-1B", unit: measurementUnits[0], size: 1.5 },
  { id: 2, name: "E-1C", unit: measurementUnits[0], size: 2 },
  { id: 3, name: "E-2A", unit: measurementUnits[0], size: 0.5 },
  { id: 4, name: "E-2B", unit: measurementUnits[0], size: 0.5 },
  { id: 5, name: "E-3A", unit: measurementUnits[0], size: 2 },
  { id: 6, name: "E-3B", unit: measurementUnits[0], size: 1 },
  { id: 7, name: "E-3C", unit: measurementUnits[0], size: 3 },
];

export const projects: Project[] = [
  { id: 0, name: "56442", description: "project 0", locations: [ locations[0], locations[1], locations[2] ] },
  { id: 1, name: "56444", description: "project 1", locations: [ locations[3], locations[4] ] },
  { id: 2, name: "56445", description: "project 2", locations: [ locations[5], locations[6], locations[7] ] },
];

export const timeSheets: TimeSheet[] = [
  { 
    id: "ckg8is43q0000fwn1fi134ghu", 
    location: locations[0],
    project: projects[0],
    workItems: [
      { id: "ckg8ise4e0000qon1hy89dpcb", action: actions[0], quantity: 1 },
      { id: "ckg8mpf0u00007gn1foao7yc1", action: actions[7], quantity: 3 },
      { id: "ckg8mpkir0000fgn19n0icbh1", action: actions[3], quantity: 1 },
    ]
  },
  {
    id: "ckg8mpq8y0000z0n11u9s5gw0", 
    location: locations[3],
    project: projects[1],
    workItems: [
      { id: "ckg8mpycu0000cwn1br5b6cpj", action: actions[4], quantity: 500 },
      { id: "ckg8mq3fx00008sn18pa9fghe", action: actions[6], quantity: 7 },
    ]
  },
];