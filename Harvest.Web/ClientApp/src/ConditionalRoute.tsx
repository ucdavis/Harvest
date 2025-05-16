import React, { useContext } from "react";

import { Route, RouteProps } from "react-router-dom";

import AppContext from "./Shared/AppContext";
import { RoleName } from "./types";

interface ConditionalRouteProps extends RouteProps {
  roles: RoleName[];
}

export const ConditionalRoute = (props: ConditionalRouteProps) => {
  const userRoles = useContext(AppContext).user.roles;

  // if the user doesn't have the shared role, add it
  if (!userRoles.includes("Shared")) {
    //TODO: If i'm doing it here, I can probably remove it everywhere else. If I don't do it here, and the shared goes to the view quote from the project details, they can see it, but if they refresh the page it gives them a not authorized below.
    // Maybe I should just remove the roles from the route in the App.tsx. If so, then I'd need to do it for the view invoice as well
    userRoles.push("Shared");
  }

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
