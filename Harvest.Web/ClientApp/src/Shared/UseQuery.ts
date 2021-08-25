import React from "react";
import 'url-search-params-polyfill';
import { useLocation } from "react-router-dom";

export function useQuery() {
  return new URLSearchParams(useLocation().search);
}