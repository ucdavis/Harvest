import React, { useContext, useMemo } from "react";

import AppContext from "./AppContext";
import { RoleName } from "../types";
import { isBoolean, isFunction } from "../Util/TypeChecks";

interface Props {
  children: any;
  roles: RoleName[];
  condition?: boolean | (() => boolean);
}

export const ShowFor = (props: Props) => {
  const { children, roles } = props;
  const userRoles = useContext(AppContext).user.roles;
  const conditionSatisfied =
    isBoolean(props.condition)
      ? props.condition
      : (isFunction(props.condition)
        ? props.condition()
        : true);

  const anyMatchingRoles: boolean = useMemo(
    () => userRoles.some((role) => roles.includes(role)),
    [props.roles]
  );

  if (
    conditionSatisfied && (userRoles.includes("System") || anyMatchingRoles)
  ) {
    return <>{children}</>;
  }

  return null;
};
