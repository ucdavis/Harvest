import * as React from "react";
import { useEffect, useState } from "react";
import { Input, InputProps } from "reactstrap";

interface IProps extends InputProps {
  value: string | number;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

const StatefulInput = (props: IProps) => {
  const [internalValue, setInternalValue] = useState(
    props.value.toString() ?? ""
  );

  useEffect(() => {
    // if parent changes value, update our internal value
    // this could happen on a slow load, or if the parent zero's out the value
    // this is not best practices for how to handle state (should have one source of truth)
    // but it's the easiest way to have a string input for our number field (for floats) and not change the type
    if (props.value.toString() !== internalValue) {
      setInternalValue(props.value.toString());
    }
  }, [props.value, internalValue]);

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
