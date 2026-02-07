public record PaginationDTO
{
  public int Page { get; init; }
  public int PageSize { get; init; }
  public int Total { get; init; }
  public int TotalPages { get; init; }
}

public record PagedResponseDTO<T>
{
  public required List<T> Data { get; init; }
  public required PaginationDTO Pagination { get; init; }
}