import { useMemo } from "react";
import { Cell, Column, TableState } from "react-table";
import { ReactTable } from "../Shared/ReactTable";
import { ReactTableUtil } from "../Shared/TableUtil";
import { History } from "../types";

interface Props {
  histories: History[];
  compact?: boolean;
}

export const HistoryTable = (props: Props) => {
  const historyData = useMemo(() => props.histories, [props.histories]);

  const columns: Column<History>[] = useMemo(
    () => [
      {
        Header: "Description",
        accessor: (row) => row.description,
        Cell: (data: Cell<History>) => {
          const description = data.row.original.description;
          if (!description) return "";

          // Split by line breaks and render each line
          const lines = description.split(/\r?\n/);
          return (
            <div style={{ whiteSpace: "pre-wrap" }}>
              {lines.map((line, index) => (
                <div key={index}>{line}</div>
              ))}
            </div>
          );
        },
      },
      {
        id: "actionDate",
        Header: "Date",
        accessor: (row) =>
          `${row.actionDate} ${new Date(row.actionDate).toLocaleDateString()}`,
        Cell: (data: Cell<History>) =>
          new Date(data.row.original.actionDate).toLocaleDateString(),
      },
      {
        Header: "Actor",
        accessor: (row) => row.actor?.name,
      },
    ],
    []
  );

  const initialState: Partial<TableState<any>> = {
    sortBy: [{ id: "actionDate", desc: true }],
    pageSize: ReactTableUtil.getPageSize(),
  };

  return (
    <ReactTable
      columns={columns}
      data={historyData}
      initialState={initialState}
      hideFilters={props.compact || false}
      hidePagination={props.compact}
    />
  );
};
