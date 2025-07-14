import { useMemo } from "react";
import { Column, TableState } from "react-table";
import { ReactTable } from "../Shared/ReactTable";
import { ReactTableUtil } from "../Shared/TableUtil";
import { ProjectPermission } from "../types";
import { convertCamelCase } from "../Util/StringFormatting";

interface Props {
  projectPermissions: ProjectPermission[];
}

export const ProjectPermissionTable = (props: Props) => {
  const permissionData = useMemo(
    () => props.projectPermissions,
    [props.projectPermissions]
  );

  const columns: Column<ProjectPermission>[] = useMemo(
    () => [
      // {
      //   Header: "Id",
      //   accessor: (row) => row.id,
      // },
      {
        Header: "Access",
        accessor: (row) => convertCamelCase(row.permission),
      },
      {
        Header: "User",
        accessor: (row) => `${row.user.name} (${row.user.email})`,
      },
      //Need to add a column to delete permissions
      // {
      //   Header: "Actions",
      //   Cell: (row) => (
      //     <button
      //       className="btn btn-danger"
      //       onClick={() => deletePermission(row.row.original.id)}
      //     >
      //       Delete
      //     </button>
      //   ),
      // },
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
      data={permissionData}
      initialState={initialState}
      hideFilters={false}
      hidePagination={false}
    />
  );
};
