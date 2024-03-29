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
} from "@fortawesome/free-solid-svg-icons";

export const ReactTable = ({
  columns,
  data,
  initialState,
  filterTypes,
  hideFilters = false,
  hidePagination = false,
}: any) => {
  const defaultColumn = React.useMemo(
    () => ({
      Filter: DefaultColumnFilter,
    }),
    []
  );

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
    state: { pageIndex, pageSize },
  } = useTable(
    {
      columns,
      data,
      defaultColumn,
      initialState: { ...initialState, pageIndex: 0 },
      filterTypes,
      autoResetSortBy: false,
      autoResetFilters: false,
    },
    useFilters, // useFilters!
    useGlobalFilter, // useGlobalFilter!
    useSortBy,
    usePagination
  );

  return (
    <>
      <table
        className="table harvest-table table-bordered table-striped"
        {...getTableProps()}
      >
        <thead>
          {headerGroups.map((headerGroup) => (
            <tr className="table-row" {...headerGroup.getHeaderGroupProps()}>
              {headerGroup.headers.map((column) => (
                <th
                  {...column.getHeaderProps(column.getSortByToggleProps())}
                  className={`sort-${
                    column.isSorted
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
                <tr className="rt-tr-group" {...row.getRowProps()}>
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
                <tr className="rt-tr-group" {...row.getRowProps()}>
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
