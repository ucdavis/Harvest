import React, { useContext } from "react";

import { Route, RouteProps } from "react-router-dom";

import AppContext from "./Shared/AppContext";
import { RoleName } from "./types";

interface ConditionalRouteProps extends RouteProps {
  roles: RoleName[];
}

export const ConditionalRoute = (props: ConditionalRouteProps) => {
  const userRoles = useContext(AppContext).user.roles;

  // if the user has System role they can see everything
  if (userRoles.includes("System")) {
    return <Route {...props} />;
  }

  const anyMatchingRoles = userRoles.some((role) => props.roles.includes(role));

  if (anyMatchingRoles) {
    // user has permission to see route
    return <Route {...props} />;
  } else {
    return <Route {...props} component={Restricted}></Route>;
  }
};

const Restricted = () => (
  <div>Sorry, you don't have access to see this page</div>
);
