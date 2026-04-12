export function getCarImageTypeLabel(imageType?: number | null): string {
  switch (imageType) {
    case 0:
      return "Спереди";
    case 1:
      return "Сбоку";
    case 2:
      return "Салон";
    case 3:
      return "Сзади";
    default:
      return "Общий вид";
  }
}
