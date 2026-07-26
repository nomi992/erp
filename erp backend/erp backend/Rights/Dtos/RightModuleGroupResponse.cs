namespace erp_backend.Rights.Dtos;

public class RightModuleGroupResponse
{
    public string Module { get; set; } = string.Empty;
    public List<RightResponse> Rights { get; set; } = [];
}
