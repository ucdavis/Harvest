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

  const onSelect = (selected: (string | { label: string })[]) => {
    const selectedStrings = selected.map((s) =>
      typeof s === "string" ? s : s.label
    );
    if (selectedStrings && selectedStrings.length > 0) {
      props.onChange(selectedStrings.join(","));
    } else {
      props.onChange(""); //If it is cleared out...
    }
  };

  useEffect(() => {
    const onSearch = async () => {
      setIsSearchLoading(true);

      const response = await fetch(`/api/Crop/Search?type=${props.cropType}`);

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

    onSearch();
  }, [props.cropType, getIsMounted]);

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
      isLoading={isSearchLoading}
    />
  );
};
