import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { authenticatedFetch } from "../Util/Api";
import { formatCurrency } from "../Util/NumberFormatting";
import { useIsMounted } from "../Shared/UseIsMounted";
import { CommonRouteParams } from "../types";

interface Props {
  projectId: number;
  remaining: number;
  shareId?: string;
}

export const ProjectUnbilledButton = (props: Props) => {
  const [total, setTotal] = useState<number>();
  const { team } = useParams<CommonRouteParams>();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await authenticatedFetch(
        `/api/${team}/expense/getunbilledtotal/${props.projectId}`
      );

      if (response.ok) {
        getIsMounted() && setTotal(Number.parseFloat(await response.text()));
      }
    };

    cb();
  }, [props.projectId, getIsMounted, team]);

  if (total === 0) {
    return (
      <button className="btn btn-lg btn-outline-disabled" disabled>
        Unbilled Expenses - $0.00
      </button>
    );
  }
  if (total === undefined || (total !== 0 && total <= props.remaining)) {
    return (
      <Link
        to={
          props.shareId
            ? `/${team}/expense/unbilled/${props.projectId}/${props.shareId}`
            : `/${team}/expense/unbilled/${props.projectId}`
        }
        className="btn btn-lg btn-outline"
      >
        View Unbilled Expenses - $
        {total === undefined ? "xx.xx" : formatCurrency(total)}
      </Link>
    );
  }

  return (
    <Link
      to={
        props.shareId
          ? `/${team}/expense/unbilled/${props.projectId}/${props.shareId}`
          : `/${team}/expense/unbilled/${props.projectId}`
      }
      className="btn btn-lg  btn-outline-danger"
    >
      View Unbilled Expenses - $
      {total === undefined ? "xx.xx" : formatCurrency(total)}
    </Link>
  );
};
