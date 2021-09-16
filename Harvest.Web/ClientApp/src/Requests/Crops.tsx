import React, { useEffect, useState } from "react";

import { Typeahead, TypeaheadProps } from "react-bootstrap-typeahead";
import { Crop, CropType } from "../types";
import { useIsMounted } from "../Shared/UseIsMounted";

interface Props extends Pick<TypeaheadProps<string>, "onBlur"> {
  crops: string;
  onChange: (crops: string) => void;
  cropType: CropType;
}

const splitCrops = (crop: string) => (crop ? crop.split(",") : []);

export const Crops = (props: Props) => {
  const [crops, onChange] = useState<string[]>(splitCrops(props.crops));
  const [options, setOptions] = useState<string[]>([]);
  const [isSearchLoading, setIsSearchLoading] = useState<boolean>(false);
  const getIsMounted = useIsMounted();

  useEffect(() => {
    if (!props.crops) {
      onChange([]);
    } else {
      onChange(splitCrops(props.crops));
    }
  }, [props.crops]);

  useEffect(() => {
    onSearch();
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

  const onSearch = async (query?: string) => {
    setIsSearchLoading(true);

    const response = await fetch(
      `/Crop/Search?type=${props.cropType}${(query && `&query=${query}`) || ""}`
    );

    if (response.ok) {
      if (response.status === 204) {
        getIsMounted() && setOptions([]); // no content means no match
      } else {
        const crops: string[] = (await response.json()).map(
          (c: Crop) => c.name
        );

        getIsMounted() && setOptions(crops);
      }
    }
    getIsMounted() && setIsSearchLoading(false);
  };

  return (
    <Typeahead<string>
      id="crops" // for accessibility
      allowNew
      multiple
      defaultSelected={crops}
      options={options}
      placeholder="Search for common crops or add your own"
      onChange={onSelect}
      onBlur={props.onBlur}
      isLoading={isSearchLoading}
    />
  );
};
