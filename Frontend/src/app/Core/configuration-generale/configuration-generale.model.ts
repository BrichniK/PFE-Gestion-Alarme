export interface ConfigurationGeneraleDto {
    configurationGeneraleId: string | null;
    ecraserEmployeMaintenance: boolean;
    accepterSeulementEmployesPlanifies: boolean;
    diagnostiqueObligatoire: boolean;
    monitoringPourcentageSurSommeDurees: boolean;
    coefficientGaugeD1: number;
    coefficientGaugeD2: number;
    coefficientGaugeD3: number;
    coefficientGaugeD4: number;
}
