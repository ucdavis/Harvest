import * as React from "react";
import {
  useTable,
  useFilters,
  useGlobalFilter,
  useSortBy,
  usePagination,
} from "react-table";
import { ColumnFilterHeaders, DefaultColumnFilter } from "./Filtering";
import { PaginationItem, PaginationLink } from "reactstrap";
import { ReactTableUtil } from "./TableUtil";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faLongArrowAltUp,
  faLongArrowAltDown,
  faDownload,
} from "@fortawesome/free-solid-svg-icons";

const loadPersistedTableState = (
  stateStorageKey: string | undefined,
  stateStorageTtlMs: number
) => {
  if (!stateStorageKey || typeof window === "undefined") {
    return undefined;
  }

  try {
    const rawState = window.localStorage.getItem(stateStorageKey);
    if (!rawState) {
      return undefined;
    }

    const parsedState = JSON.parse(rawState);

    if (
      typeof parsedState?.savedAt !== "number" ||
      typeof parsedState?.state !== "object" ||
      parsedState.state === null
    ) {
      window.localStorage.removeItem(stateStorageKey);
      return undefined;
    }

    if (Date.now() - parsedState.savedAt > stateStorageTtlMs) {
      window.localStorage.removeItem(stateStorageKey);
      return undefined;
    }

    return parsedState.state;
  } catch {
    window.localStorage.removeItem(stateStorageKey);
    return undefined;
  }
};

const savePersistedTableState = (
  stateStorageKey: string | undefined,
  tableState: any
) => {
  if (!stateStorageKey || typeof window === "undefined") {
    return;
  }

  try {
    window.localStorage.setItem(
      stateStorageKey,
      JSON.stringify({
        savedAt: Date.now(),
        state: tableState,
      })
    );
  } catch {
    // Fail open if storage is unavailable or blocked.
  }
};

const hasActiveFilters = (tableState: any) => {
  return (
    (Array.isArray(tableState?.filters) && tableState.filters.length > 0) ||
    Boolean(tableState?.globalFilter)
  );
};

const getRestoredStateAlertMessage = (tableState: any) => {
  if (!tableState) {
    return "";
  }

  if (hasActiveFilters(tableState)) {
    return "Filters from your last visit were restored, so this list may not show every result.";
  }

  return "";
};

export const ReactTable = ({
  columns,
  data,
  initialState,
  filterTypes,
  hideFilters = false,
  hidePagination = false,
  onRowClick,
  enableExport = false,
  stateStorageKey,
  stateStorageTtlMs = 30 * 60 * 1000,
}: any) => {
  const defaultColumn = React.useMemo(
    () => ({
      Filter: DefaultColumnFilter,
    }),
    []
  );

  const restoredTableState = React.useMemo(() => {
    return loadPersistedTableState(
      stateStorageKey,
      stateStorageTtlMs
    );
  }, [stateStorageKey, stateStorageTtlMs]);

  const persistedInitialState = React.useMemo(() => {
    const restoredState = restoredTableState;

    if (!restoredState) {
      return initialState;
    }

    return {
      ...initialState,
      ...restoredState,
    };
  }, [initialState, restoredTableState]);

  const {
    getTableProps,
    getTableBodyProps,
    headerGroups,
    rows,
    prepareRow,
    // pagination
    page,
    canPreviousPage,
    canNextPage,
    pageOptions,
    pageCount,
    gotoPage,
    nextPage,
    previousPage,
    setPageSize,
    setAllFilters,
    setGlobalFilter,
    state: { filters, globalFilter, sortBy, pageIndex, pageSize },
  } = useTable(
    {
      columns,
      data,
      defaultColumn,
      initialState: {
        ...persistedInitialState,
        pageIndex: persistedInitialState?.pageIndex ?? 0,
      },
      filterTypes,
      autoResetSortBy: false,
      autoResetFilters: false,
    },
    useFilters, // useFilters!
    useGlobalFilter, // useGlobalFilter!
    useSortBy,
    usePagination
  );

  const persistedTableStateRef = React.useRef<any>();
  const hasInitializedAlertRef = React.useRef(false);

  React.useEffect(() => {
    const tableState = {
      filters,
      globalFilter,
      sortBy,
      pageIndex,
      pageSize,
    };

    persistedTableStateRef.current = tableState;
    savePersistedTableState(stateStorageKey, tableState);
  }, [
    filters,
    globalFilter,
    pageIndex,
    pageSize,
    sortBy,
    stateStorageKey,
  ]);

  React.useEffect(() => {
    return () => {
      savePersistedTableState(stateStorageKey, persistedTableStateRef.current);
    };
  }, [stateStorageKey]);

  const currentTableState = React.useMemo(
    () => ({
      filters,
      globalFilter,
      sortBy,
      pageIndex,
      pageSize,
    }),
    [filters, globalFilter, pageIndex, pageSize, sortBy]
  );

  const [showRestoredStateAlert, setShowRestoredStateAlert] = React.useState(
    () => Boolean(getRestoredStateAlertMessage(restoredTableState))
  );

  React.useEffect(() => {
    setShowRestoredStateAlert(
      Boolean(getRestoredStateAlertMessage(restoredTableState))
    );
    hasInitializedAlertRef.current = false;
  }, [restoredTableState, stateStorageKey]);

  React.useEffect(() => {
    if (!hasInitializedAlertRef.current) {
      hasInitializedAlertRef.current = true;
      return;
    }

    setShowRestoredStateAlert(false);
  }, [filters, globalFilter]);

  const restoredStateAlertMessage = showRestoredStateAlert
    ? getRestoredStateAlertMessage(restoredTableState)
    : "";

  const handleClearFilters = React.useCallback(() => {
    setAllFilters([]);
    setGlobalFilter(undefined);
    gotoPage(0);
    setShowRestoredStateAlert(false);
  }, [gotoPage, setAllFilters, setGlobalFilter]);

  // CSV Export functionality
  const exportToCSV = () => {
    // Get all column headers (visible columns only)
    const headers = columns
      .filter((col: any) => col.accessor) // Only include columns with accessors
      .map((col: any) => col.Header || col.accessor);

    // Convert data to CSV format
    const csvData = [
      headers.join(","), // Header row
      ...data.map((row: any) =>
        columns
          .filter((col: any) => col.accessor)
          .map((col: any) => {
            let value = "";
            if (typeof col.accessor === "string") {
              value = row[col.accessor] || "";
            } else if (typeof col.accessor === "function") {
              value = col.accessor(row) || "";
            }

            // Handle values that contain commas, quotes, or newlines
            if (
              typeof value === "string" &&
              (value.includes(",") ||
                value.includes('"') ||
                value.includes("\n"))
            ) {
              value = `"${value.replace(/"/g, '""')}"`;
            }

            return value;
          })
          .join(",")
      ),
    ].join("\n");

    // Create and download the file
    const now = new Date();
    const timestamp = now.toISOString().replace(/[:.]/g, "-").slice(0, 19);
    const filename = `export-${timestamp}.csv`;

    const blob = new Blob([csvData], { type: "text/csv;charset=utf-8;" });
    const link = document.createElement("a");

    if (link.download !== undefined) {
      const url = URL.createObjectURL(blob);
      link.setAttribute("href", url);
      link.setAttribute("download", filename);
      link.style.visibility = "hidden";
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
    }
  };

  return (
    <>
      {restoredStateAlertMessage && (
        <div className="alert alert-info d-flex justify-content-between align-items-center">
          <span>{restoredStateAlertMessage}</span>
          {hasActiveFilters(currentTableState) && (
            <button
              type="button"
              className="btn btn-sm btn-outline-info ms-3"
              onClick={handleClearFilters}
            >
              Clear Filters
            </button>
          )}
        </div>
      )}
      {enableExport && (
        <div className="mb-2">
          <button
            className="btn btn-link p-0 text-primary"
            onClick={exportToCSV}
          >
            <FontAwesomeIcon icon={faDownload} className="me-1" /> Export CSV
          </button>
        </div>
      )}
      <table
        className="table harvest-table"
        {...getTableProps()}
      >
        <thead>
          {headerGroups.map((headerGroup) => (
            <tr className="table-row" {...headerGroup.getHeaderGroupProps()}>
              {headerGroup.headers.map((column) => (
                <th
                  {...column.getHeaderProps(column.getSortByToggleProps())}
                  className={`sort-${column.isSorted
                    ? column.isSortedDesc
                      ? "desc"
                      : "asc"
                    : "none"
                    }`}
                >
                  {column.render("Header")}
                  {/* Render the columns filter UI */}
                  <span>
                    {column.isSorted ? (
                      column.isSortedDesc ? (
                        <FontAwesomeIcon icon={faLongArrowAltDown} />
                      ) : (
                        <FontAwesomeIcon icon={faLongArrowAltUp} />
                      )
                    ) : (
                      ""
                    )}
                  </span>
                </th>
              ))}
            </tr>
          ))}
          {hideFilters === false && (
            <ColumnFilterHeaders headerGroups={headerGroups} />
          )}
        </thead>
        <tbody {...getTableBodyProps()}>
          {/* This will reveal all rows if hidePagination is true */}
          {hidePagination === true &&
            rows.map((row) => {
              prepareRow(row);
              return (
                <tr
                  className="rt-tr-group"
                  {...row.getRowProps()}
                  onClick={
                    onRowClick
                      ? (e) => {
                        // Don't trigger row click if clicking on a button or link
                        const target = e.target as HTMLElement;
                        if (
                          target.tagName === "BUTTON" ||
                          target.tagName === "A" ||
                          target.closest("button") ||
                          target.closest("a")
                        ) {
                          return;
                        }
                        onRowClick(row.original);
                      }
                      : undefined
                  }
                >
                  {row.cells.map((cell) => {
                    return (
                      <td {...cell.getCellProps()}>{cell.render("Cell")}</td>
                    );
                  })}
                </tr>
              );
            })}
          {/* This will paginate the data if hidePagination is false */}
          {hidePagination === false &&
            page.map((row) => {
              prepareRow(row);
              return (
                <tr
                  className="rt-tr-group"
                  {...row.getRowProps()}
                  onClick={
                    onRowClick
                      ? (e) => {
                        // Don't trigger row click if clicking on a button or link
                        const target = e.target as HTMLElement;
                        if (
                          target.tagName === "BUTTON" ||
                          target.tagName === "A" ||
                          target.closest("button") ||
                          target.closest("a")
                        ) {
                          return;
                        }
                        onRowClick(row.original);
                      }
                      : undefined
                  }
                  style={
                    onRowClick
                      ? {
                        cursor: "pointer",
                        transition: "background-color 0.2s",
                      }
                      : undefined
                  }
                  onMouseEnter={
                    onRowClick
                      ? (e) => {
                        e.currentTarget.style.backgroundColor = "#f8f9fa";
                      }
                      : undefined
                  }
                  onMouseLeave={
                    onRowClick
                      ? (e) => {
                        e.currentTarget.style.backgroundColor = "";
                      }
                      : undefined
                  }
                >
                  {row.cells.map((cell) => {
                    return (
                      <td {...cell.getCellProps()}>{cell.render("Cell")}</td>
                    );
                  })}
                </tr>
              );
            })}
        </tbody>
      </table>
      {hidePagination === true ? null : (
        <div className="pagination justify-content-center">
          <PaginationItem
            className="align-self-center"
            onClick={() => gotoPage(0)}
            disabled={!canPreviousPage}
          >
            <PaginationLink first />
          </PaginationItem>
          <PaginationItem
            className="align-self-center"
            onClick={() => previousPage()}
            disabled={!canPreviousPage}
          >
            <PaginationLink previous />
          </PaginationItem>
          <PaginationItem>
            <PaginationLink>
              <span>
                Page{" "}
                <strong>
                  {pageIndex + 1} of {pageOptions.length}
                </strong>{" "}
              </span>
              <span>
                | Go to page:{" "}
                <input
                  className="form-control d-inline"
                  type="number"
                  defaultValue={pageIndex + 1}
                  onChange={(e) => {
                    const page = e.target.value
                      ? Number(e.target.value) - 1
                      : 0;
                    gotoPage(page);
                  }}
                  style={{ width: "100px" }}
                />
              </span>{" "}
            </PaginationLink>
          </PaginationItem>
          <PaginationItem>
            <PaginationLink>
              <select
                className="form-control"
                value={pageSize}
                onChange={(e) => {
                  ReactTableUtil.setPageSize(e.target.value);
                  setPageSize(Number(e.target.value));
                }}
              >
                {[10, 20, 30, 40, 50].map((pageSize) => (
                  <option key={pageSize} value={pageSize}>
                    Show {pageSize}
                  </option>
                ))}
              </select>{" "}
            </PaginationLink>
          </PaginationItem>
          <PaginationItem
            className="align-self-center"
            onClick={() => nextPage()}
            disabled={!canNextPage}
          >
            <PaginationLink next />
          </PaginationItem>
          <PaginationItem
            className="align-self-center"
            onClick={() => gotoPage(pageCount - 1)}
            disabled={!canNextPage}
          >
            <PaginationLink last />
          </PaginationItem>
        </div>
      )}
    </>
  );
};
