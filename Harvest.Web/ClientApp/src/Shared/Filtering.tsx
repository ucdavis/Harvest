import React, { useState, useRef, useEffect } from "react";
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
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [isOpen]);

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
  const selectedValues = filterValue || [];

  const handleToggle = (value: any) => {
    if (value === "") {
      // "All" was clicked, clear the filter
      setFilter(undefined);
    } else {
      const currentSelected = filterValue || [];
      if (currentSelected.includes(value)) {
        // Remove the value
        const newSelected = currentSelected.filter((v: any) => v !== value);
        setFilter(newSelected.length > 0 ? newSelected : undefined);
      } else {
        // Add the value
        setFilter([...currentSelected, value]);
      }
    }
  };

  const displayText =
    !selectedValues || selectedValues.length === 0
      ? "All"
      : selectedValues.length === 1
      ? convertCamelCase(String(selectedValues[0]))
      : `${selectedValues.length} selected`;

  return (
    <div ref={dropdownRef} style={{ position: "relative" }}>
      <button
        className="form-control"
        onClick={() => setIsOpen(!isOpen)}
        style={{
          textAlign: "left",
          backgroundColor: "white",
          border: "1px solid #ced4da",
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          width: "100%",
        }}
        type="button"
      >
        <span
          style={{
            overflow: "hidden",
            textOverflow: "ellipsis",
            whiteSpace: "nowrap",
            flex: 1,
          }}
        >
          {displayText}
        </span>
        <span style={{ marginLeft: "8px", flexShrink: 0 }}>▼</span>
      </button>
      {isOpen && (
        <div
          style={{
            position: "absolute",
            top: "100%",
            left: 0,
            minWidth: "200px",
            width: "max-content",
            backgroundColor: "white",
            border: "1px solid #ced4da",
            borderRadius: "4px",
            marginTop: "2px",
            maxHeight: "250px",
            overflowY: "auto",
            zIndex: 1000,
            boxShadow: "0 2px 4px rgba(0,0,0,0.2)",
          }}
        >
          <div style={{ padding: "8px 12px" }}>
            <input
              type="checkbox"
              id={`${id}-filter-all`}
              checked={!selectedValues || selectedValues.length === 0}
              onChange={() => {
                setFilter(undefined);
                setIsOpen(false);
              }}
              style={{ marginRight: "8px", cursor: "pointer" }}
            />
            <label
              htmlFor={`${id}-filter-all`}
              style={{ cursor: "pointer", marginBottom: 0 }}
            >
              All
            </label>
          </div>
          {options.map((option: any) => {
            const optionKey = String(option);
            const displayValue =
              typeof option === "string"
                ? convertCamelCase(option)
                : String(option);

            return (
              <div
                key={`${id}-filter-${optionKey}`}
                style={{ padding: "8px 12px" }}
              >
                <input
                  type="checkbox"
                  id={`${id}-filter-${optionKey}`}
                  checked={selectedValues.includes(option)}
                  onChange={() => handleToggle(option)}
                  style={{ marginRight: "8px", cursor: "pointer" }}
                />
                <label
                  htmlFor={`${id}-filter-${optionKey}`}
                  style={{ cursor: "pointer", marginBottom: 0 }}
                >
                  {displayValue}
                </label>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};

// Custom filter function for multi-select
export const multiSelectFilter = (rows: any[], id: any, filterValue: any) => {
  // If filterValue is undefined or empty, show all rows
  if (!filterValue || filterValue.length === 0) {
    return rows;
  }

  // Filter rows where the cell value matches any of the selected values
  return rows.filter((row) => {
    const rowValue = row.values[id];
    return filterValue.includes(rowValue);
  });
};

export const SelectColumnFilterRange = ({
  column: { filterValue, setFilter, preFilteredRows, id },
}: any) => {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [isOpen]);

  const options = [
    { value: "all", label: "All" },
    { value: "rangeOne", label: "<25%" },
    { value: "rangeTwo", label: "25-50%" },
    { value: "rangeThree", label: "50-75%" },
    { value: "rangeFour", label: ">75%" },
  ];

  const displayText =
    options.find((opt) => opt.value === filterValue)?.label || "All";

  return (
    <div ref={dropdownRef} style={{ position: "relative" }}>
      <button
        className="form-control"
        onClick={() => setIsOpen(!isOpen)}
        style={{
          textAlign: "left",
          backgroundColor: "white",
          border: "1px solid #ced4da",
          overflow: "hidden",
          textOverflow: "ellipsis",
          whiteSpace: "nowrap",
          display: "block",
          width: "100%",
        }}
        type="button"
      >
        {displayText} <span style={{ float: "right" }}>▼</span>
      </button>
      {isOpen && (
        <div
          style={{
            position: "absolute",
            top: "100%",
            left: 0,
            minWidth: "200px",
            width: "max-content",
            backgroundColor: "white",
            border: "1px solid #ced4da",
            borderRadius: "4px",
            marginTop: "2px",
            maxHeight: "250px",
            overflowY: "auto",
            zIndex: 1000,
            boxShadow: "0 2px 4px rgba(0,0,0,0.2)",
          }}
        >
          {options.map((option, i) => (
            <div
              key={i}
              style={{
                padding: "8px 12px",
                cursor: "pointer",
                backgroundColor:
                  filterValue === option.value ? "#f0f0f0" : "white",
              }}
              onClick={() => {
                setFilter(option.value === "all" ? undefined : option.value);
                setIsOpen(false);
              }}
              onMouseEnter={(e) =>
                (e.currentTarget.style.backgroundColor = "#f8f9fa")
              }
              onMouseLeave={(e) =>
                (e.currentTarget.style.backgroundColor =
                  filterValue === option.value ? "#f0f0f0" : "white")
              }
            >
              {option.label}
            </div>
          ))}
        </div>
      )}
    </div>
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
