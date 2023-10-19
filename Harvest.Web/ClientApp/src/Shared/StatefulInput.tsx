import * as React from "react";
import { useState } from "react";
import { Input, InputProps } from "reactstrap";

interface IProps extends InputProps {
  value: string | number;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

const StatefulInput = (props: IProps) => {
  const [internalValue, setInternalValue] = useState(
    props.value.toString() ?? ""
  );
  return (
    <Input
      {...props}
      value={internalValue}
      onChange={(e) => {
        setInternalValue(e.target.value);
        props.onChange(e);
      }}
    />
  );
};

export default StatefulInput;
