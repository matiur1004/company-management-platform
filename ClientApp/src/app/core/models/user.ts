export interface IUser {
    Id: number;
    UserName: string;
    Password: string;
    FirstName: string;
    LastName: string;
    JwtToken?: string;
  }
  
  export interface IopUser {
    Id: number;
    UmfaId: number;
    FirstName: string;
    LastName: string;
    UserName: string;
    IsAdmin: boolean;
    AmrScadaUsers?: IAmrUser[];
  }
  
  export interface IAmrUser {
    Id: number;
    ProfileName: string;
    ScadaUserName: string;
    ScadaPassword: string;
    SgdUrl: string;
    Active: boolean;
  }
  