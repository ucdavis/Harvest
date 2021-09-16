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

  const onSelect = (selected: string[]) => {
    if (selected && selected.length > 0) {
      props.onChange(selected.join(","));
    } else {
      props.onChange(""); //If it is cleared out...
    }
  };

  const onSearch = async () => {
    setIsSearchLoading(true);

    const response = await fetch(`/Crop/Search?type=${props.cropType}`);

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
