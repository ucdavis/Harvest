export class ReactTableUtil {
  public static setPageSize(pageSize: string) {
    localStorage.setItem("HarvestDefaultPageSize", pageSize);
  }

  public static getPageSize(): number {
    return (
      parseInt(localStorage.getItem("HarvestDefaultPageSize") || "20") || 20
    );
  }
}
