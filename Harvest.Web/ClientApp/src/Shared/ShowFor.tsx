import React from "react";

interface Props {
  show: boolean;
  children: any;
}

export const ShowFor = (props: Props) => {
  const { show, children } = props;

  if (show === false) {
      return null;
  }
  return <div>{children}</div>;
};
