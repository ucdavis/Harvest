import { useMemo } from "react";
import { useParams } from "react-router-dom";
import { Cell, Column, TableState } from "react-table";
import { ReactTable } from "../Shared/ReactTable";
import { ReactTableUtil } from "../Shared/TableUtil";
import { History, CommonRouteParams } from "../types";

interface Props {
  histories: History[];
  compact?: boolean;
}

export const HistoryTable = (props: Props) => {
  const historyData = useMemo(() => props.histories, [props.histories]);
  const { team } = useParams<CommonRouteParams>();
  const columns: Column<History>[] = useMemo(
    () => [
      {
        Header: "Description",
        accessor: (row) => row.description,
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
        accessor: (row) => row.actor.name,
      },
    ],
    [team]
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
