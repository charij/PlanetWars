

module Server {
    
    export interface StatusResult {
        
        isGameOver: boolean;
        status: boolean;
        playerA: number;
        playerB: number;
        playerAScore: number;
        playerBScore: number;
        currentTurn: number;
        endOfCurrentTurn: Date;
        nextTurnStart: Date;
        playerTurnLength: number;
        serverTurnLength: number;
        planets: Planet[];
        fleets: Fleet[];
    }
}