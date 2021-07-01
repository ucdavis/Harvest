import React, { useContext, useMemo } from "react";

import AppContext from "./AppContext";
import { RoleName } from "../types";

interface Props {
  children: any;
  roles: RoleName[];
}

export const ShowFor = (props: Props) => {
  const { children, roles } = props;
  const userRoles = useContext(AppContext).user.roles;

  const anyMatchingRoles: boolean = useMemo(
    () => userRoles.some((role) => roles.includes(role)),
    [...props.roles]
  );

  if (
    userRoles.includes("Admin") ||
    userRoles.includes("System") ||
    anyMatchingRoles
  ) {
    return <>{children}</>;
  }

  return null;
};
