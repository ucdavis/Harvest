import { useMemo } from "react";
import { Column, TableState } from "react-table";
import { ReactTable } from "../Shared/ReactTable";
import { ProjectPermission } from "../types";
import { convertCamelCase } from "../Util/StringFormatting";
import { Button } from "reactstrap";

interface Props {
  projectPermissions: ProjectPermission[];
  deletePermission: (permission: ProjectPermission) => void;
  canDeletePermission: boolean;
}

export const ProjectPermissionTable = (props: Props) => {
  const permissionData = useMemo(
    () => props.projectPermissions,
    [props.projectPermissions]
  );

  const { deletePermission, canDeletePermission } = props;

  const columns: Column<ProjectPermission>[] = useMemo(
    () => [
      {
        Header: "Permission",
        accessor: (row) => convertCamelCase(row.permission),
      },
      {
        Header: "User",
        accessor: (row) => `${row.user.name} (${row.user.email})`,
      },
      ...(canDeletePermission
        ? [
          {
            Header: "Action",
            Cell: (data: any) => (
              <Button
                color="danger"
                size="sm"
                onClick={() => deletePermission(data.row.original)}
              >
                Delete
              </Button>
            ),
          },
        ]
        : []),
    ],
    [deletePermission, canDeletePermission]
  );

  const initialState: Partial<TableState<any>> = {
    sortBy: [{ id: "id", desc: true }],
  };

  return (
    <ReactTable
      columns={columns}
      data={permissionData}
      initialState={initialState}
      hideFilters={true}
      hidePagination={true}
    />
  );
};
