import React, { useContext, useCallback } from "react";

import AppContext from "./AppContext";
import { RoleName } from "../types";

export const useRoleChecks = () => {
  const userRoles = useContext(AppContext).user.roles;

  const anyMatchingRoles = useCallback(
    (roles: RoleName[]) => userRoles.some((role) => roles.includes(role)),
    [userRoles]
  );

  const hasAccessForRoles = useCallback(
    (roles: RoleName[]) =>
      userRoles.some((role) => roles.includes(role)) ||
      userRoles.includes("System"),
    [userRoles]
  );

  return {
    anyMatchingRoles,
    hasAccessForRoles,
  };
};
