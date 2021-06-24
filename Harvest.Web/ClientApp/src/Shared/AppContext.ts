import React from "react";
import { AppContextShape } from "../types";

const AppContext = React.createContext<AppContextShape>({
  user: { roles: [] },
});

export default AppContext;
