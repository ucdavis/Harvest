import React, { useEffect, useState } from "react";

import { Typeahead, TypeaheadProps } from "react-bootstrap-typeahead";
import { CropType } from "../types";

interface Props extends Pick<TypeaheadProps<string>, "onBlur"> {
  crops: string;
  onChange: (crops: string) => void;
  cropType: CropType;
}

const splitCrops = (crop: string) => (crop ? crop.split(",") : []);

export const Crops = (props: Props) => {
  const [crops, onChange] = useState<string[]>(splitCrops(props.crops));
  const [options, setOptions] = useState<string[]>([]);

  useEffect(() => {
    if (!props.crops) {
      onChange([]);
    } else {
      onChange(splitCrops(props.crops));
    }
  }, [props.crops]);

  useEffect(() => {
    setOptions(CommonCrops[props.cropType]);
  }, [props.cropType]);

  const onSelect = (selected: (string | { label: string })[]) => {
    if (selected && selected.length > 0) {
      // selections are either strings or an object with the string in a label prop for custom fields, so project them to just the strings we want
      const selectedStrings = selected.map((s) =>
        typeof s === "string" ? s : s.label
      );

      props.onChange(selectedStrings.join(","));
    } else {
      props.onChange(""); //If it is cleared out...
    }
  };

  return (
    <Typeahead
      id="crops" // for accessibility
      allowNew
      multiple
      defaultSelected={crops}
      options={options}
      placeholder="Search for common crops or add your own"
      onChange={onSelect}
      onBlur={props.onBlur}
    />
  );
};

// TODO: where should we keep this list?  Worth querying every time?
const CommonCrops = {
  Row: ["corn", "cabbage", "celery"],
  Tree: ["almond", "orange"],
} as { [c: string]: string[] };
