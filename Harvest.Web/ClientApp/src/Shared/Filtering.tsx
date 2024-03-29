import React, { useState } from "react";
import DatePicker from "react-datepicker";
import { Row, HeaderGroup } from "react-table";
import { convertCamelCase } from "../Util/StringFormatting";

// Define a default UI for filtering
export const GlobalFilter = ({
  preGlobalFilteredRows,
  globalFilter,
  setGlobalFilter,
}: any) => {
  const count = preGlobalFilteredRows.length;

  return (
    <span>
      Search:{" "}
      <input
        className="form-control"
        value={globalFilter || ""}
        onChange={(e) => {
          setGlobalFilter(e.target.value || undefined); // Set undefined to remove the filter entirely
        }}
        placeholder={`${count} records...`}
      />
    </span>
  );
};

// Define a default UI for filtering
export const DefaultColumnFilter = ({
  column: { filterValue, preFilteredRows, setFilter },
}: any) => {
  return (
    <input
      className="form-control"
      value={filterValue || ""}
      onChange={(e) => {
        setFilter(e.target.value || undefined); // Set undefined to remove the filter entirely
      }}
    />
  );
};

// This is a custom filter UI for selecting
// a unique option from a list
export const SelectColumnFilter = ({
  column: { filterValue, setFilter, preFilteredRows, id },
}: any) => {
  // Calculate the options for filtering
  // using the preFilteredRows
  const options = React.useMemo(() => {
    const options = new Set<any>();
    preFilteredRows.forEach((row: Row<object>) => {
      options.add(row.values[id]);
    });
    return Array.from(options);
  }, [id, preFilteredRows]);

  // Render a multi-select box
  return (
    <select
      className="form-control"
      value={filterValue}
      onChange={(e) => {
        setFilter(e.target.value || undefined);
      }}
    >
      <option value="">All</option>
      {options.map((option: any, i: number) => (
        <option key={i} value={option}>
          {convertCamelCase(option)}
        </option>
      ))}
    </select>
  );
};

export const SelectColumnFilterRange = ({
  column: { filterValue, setFilter, preFilteredRows, id },
}: any) => {
  // Render a multi-select box
  return (
    <select
      className="form-control"
      value={filterValue}
      onChange={(e) => setFilter(e.target.value || undefined)}
    >
      <option value="all">All</option>
      <option value="rangeOne">{"<25%"}</option>
      <option value="rangeTwo">25-50%</option>
      <option value="rangeThree">50-75%</option>
      <option value="rangeFour">{">75%"}</option>
    </select>
  );
};

// Function to control which rows to showwhen filtering progresses
export const progressFilter = (rows: any[], id: any, filterValue: any) => {
  // Returns ranges of progresses depending on the chosen option
  if (filterValue === "all") {
    return rows;
  } else if (filterValue === "rangeOne") {
    return rows.filter((row) => row.values.progress < 25);
  } else if (filterValue === "rangeTwo") {
    return rows.filter(
      (row) => row.values.progress > 25 && row.values.progress < 50
    );
  } else if (filterValue === "rangeThree") {
    return rows.filter(
      (row) => row.values.progress > 50 && row.values.progress < 75
    );
  } else if (filterValue === "rangeFour") {
    return rows.filter((row) => row.values.progress > 75);
  }

  return rows;
};

export const DatePickerFilter = ({
  column: { filterValue, setFilter },
}: any) => {
  const [dateRange, setDateRange] = useState([undefined, undefined]);
  const [startDate, endDate] = dateRange;

  return (
    <DatePicker
      className="form-control"
      selectsRange={true}
      startDate={startDate}
      endDate={endDate}
      onChange={(e: any) => {
        setDateRange(e);
        setFilter(e);
      }}
      isClearable
    />
  );
};

// Function to convert date format to yyyy/mm/dd for better date comparison
const formatDate = (dateString: string) => {
  const date = new Date(dateString);
  const year = date.getFullYear();
  const month = date.getMonth() + 1;
  const day = date.getDate();

  return new Date(`${year}/${month}/${day}`);
};

export const startDateFilter = (rows: any[], id: any, filterValue: any) => {
  return rows.filter((row) => {
    if (filterValue[0] && filterValue[1]) {
      const startDate = formatDate(row.values.startDate.split(" ")[0]);
      const filterDateStart = formatDate(filterValue[0]);
      const filterDateEnd = formatDate(filterValue[1]);

      return startDate >= filterDateStart && startDate <= filterDateEnd;
    } else {
      return true;
    }
  });
};

export const endDateFilter = (rows: any[], id: any, filterValue: any) => {
  return rows.filter((row) => {
    if (filterValue[0] && filterValue[1]) {
      const endDate = formatDate(row.values.endDate.split(" ")[0]);
      const filterDateStart = formatDate(filterValue[0]);
      const filterDateEnd = formatDate(filterValue[1]);

      return endDate >= filterDateStart && endDate <= filterDateEnd;
    } else {
      return true;
    }
  });
};

export const ColumnFilterHeaders = ({ headerGroups }: any) => {
  return headerGroups.map(
    (headerGroup: HeaderGroup) =>
      !!headerGroup.headers.some((header) => !!header.Filter) && (
        <tr {...headerGroup.getHeaderGroupProps()}>
          {headerGroup.headers.map((column) => (
            <th {...column.getHeaderProps()}>
              {/* Render the columns filter UI */}
              <div>
                {column.canFilter && !!column.Filter
                  ? column.render("Filter")
                  : null}
              </div>
            </th>
          ))}
        </tr>
      )
  );
};
