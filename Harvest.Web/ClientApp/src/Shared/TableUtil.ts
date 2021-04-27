export class ReactTableUtil {
  public static setPageSize(pageSize: number) {
    localStorage.setItem("HarvestDefaultPageSize", pageSize.toString());
  }

  public static getPageSize(): number {
    return (
      parseInt(localStorage.getItem("HarvestDefaultPageSize") || "20") || 20
    );
  }
}
