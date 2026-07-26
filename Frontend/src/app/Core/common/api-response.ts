export interface ApiResponse<TData>
{
    success:boolean;
    message:string;
    statusCode:number;
    validationErrors:string[],
    data?:TData;
}
