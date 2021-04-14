import React, { useEffect, useState } from "react";

import { Typeahead } from "react-bootstrap-typeahead";

interface Props {
  crops: string;
  setCrops: (crops: string) => void;
}

export const Crops = (props: Props) => {
  const [crops, setCrops] = useState<string[]>([]);

  useEffect(() => {
    if (!props.crops) {
      setCrops([]);
    } else {
      setCrops(props.crops.split(","));
    }
  }, [props.crops]);

  const onSelect = (selected: (string | { label: string })[]) => {
    if (selected && selected.length > 0) {
      // selections are either strings or an object with the string in a label prop for custom fields, so project them to just the strings we want
      const selectedStrings = selected.map((s) =>
        typeof s === "string" ? s : s.label
      );

      props.setCrops(selectedStrings.join(","));
    }
  };

  return (
    <Typeahead
      id="crops" // for accessibility
      allowNew
      multiple
      defaultSelected={crops}
      options={CommonCrops}
      placeholder="Search for common crops or add your own"
      onChange={onSelect}
    />
  );
};

// TODO: where should we keep this list?  Worth querying every time?
const CommonCrops = ["corn", "cabbage", "celery"];
