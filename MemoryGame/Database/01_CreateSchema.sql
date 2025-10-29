USE MemoryGameDb;
GO

/* --- Safe reset if you re-run during development --- */
IF OBJECT_ID('dbo.Move','U') IS NOT NULL DROP TABLE dbo.Move;
IF OBJECT_ID('dbo.Tile','U') IS NOT NULL DROP TABLE dbo.Tile;
IF OBJECT_ID('dbo.GamePlayer','U') IS NOT NULL DROP TABLE dbo.GamePlayer;
IF OBJECT_ID('dbo.Game','U') IS NOT NULL DROP TABLE dbo.Game;
IF OBJECT_ID('dbo.Card','U') IS NOT NULL DROP TABLE dbo.Card;
IF OBJECT_ID('dbo.[User]','U') IS NOT NULL DROP TABLE dbo.[User];
GO

/* --- Users (players) --- */
CREATE TABLE dbo.[User](
    UserID        INT IDENTITY(1,1) PRIMARY KEY,
    Username      NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash  NVARCHAR(200) NOT NULL,      -- hashed+salted
    Email         NVARCHAR(100) NULL,
    CreatedAt     DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

/* --- Game sessions --- */
CREATE TABLE dbo.Game(
    GameID                INT IDENTITY(1,1) PRIMARY KEY,
    CreatedAt             DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    EndedAt               DATETIME2 NULL,
    Status                NVARCHAR(20) NOT NULL
        CONSTRAINT CK_Game_Status CHECK (Status IN ('Waiting','InProgress','Completed','Aborted')),
    WinnerGamePlayerID    INT NULL               -- FK added after GamePlayer exists
);

/* --- Players in a game --- */
CREATE TABLE dbo.GamePlayer(
    GamePlayerID  INT IDENTITY(1,1) PRIMARY KEY,
    GameID        INT NOT NULL,
    UserID        INT NOT NULL,
    PlayerOrder   INT NOT NULL
        CONSTRAINT CK_GamePlayer_PlayerOrder CHECK (PlayerOrder IN (1,2)),

    CONSTRAINT FK_GamePlayer_Game  FOREIGN KEY (GameID) REFERENCES dbo.Game(GameID),
    CONSTRAINT FK_GamePlayer_User  FOREIGN KEY (UserID) REFERENCES dbo.[User](UserID),

    CONSTRAINT UQ_GamePlayer_Game_User       UNIQUE (GameID, UserID),
    CONSTRAINT UQ_GamePlayer_Game_PlayerSlot UNIQUE (GameID, PlayerOrder)
);

/* --- Card types --- */
CREATE TABLE dbo.Card(
    CardID     INT IDENTITY(1,1) PRIMARY KEY,
    [Name]     NVARCHAR(50) NOT NULL,
    ImagePath  NVARCHAR(200) NULL,
    PairKey    NVARCHAR(50) NOT NULL           -- matching pair grouping
);

/* --- Tiles on the board for a specific game --- */
CREATE TABLE dbo.Tile(
    TileID     INT IDENTITY(1,1) PRIMARY KEY,
    GameID     INT NOT NULL,
    CardID     INT NOT NULL,
    Position   INT NOT NULL,                   -- e.g. 0..15 for 4x4
    IsMatched  BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_Tile_Game FOREIGN KEY (GameID) REFERENCES dbo.Game(GameID),
    CONSTRAINT FK_Tile_Card FOREIGN KEY (CardID) REFERENCES dbo.Card(CardID),

    CONSTRAINT UQ_Tile_Game_Position UNIQUE (GameID, Position),
    CONSTRAINT UQ_Tile_Game_Tile UNIQUE (GameID, TileID)    -- target for composite FKs
);

/* --- Moves (one turn = two flips) --- */
CREATE TABLE dbo.Move(
    MoveID        INT IDENTITY(1,1) PRIMARY KEY,
    GameID        INT NOT NULL,
    UserID        INT NOT NULL,
    FirstTileID   INT NOT NULL,
    SecondTileID  INT NOT NULL,
    IsMatch       BIT NOT NULL,
    CreatedAt     DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    TurnNumber    INT NOT NULL,

    CONSTRAINT FK_Move_Game FOREIGN KEY (GameID) REFERENCES dbo.Game(GameID),
    CONSTRAINT FK_Move_User FOREIGN KEY (UserID) REFERENCES dbo.[User](UserID),

    -- Composite FKs: both tiles must belong to the SAME game
    CONSTRAINT FK_Move_FirstTile
        FOREIGN KEY (GameID, FirstTileID) REFERENCES dbo.Tile (GameID, TileID),
    CONSTRAINT FK_Move_SecondTile
        FOREIGN KEY (GameID, SecondTileID) REFERENCES dbo.Tile (GameID, TileID)
);

/* --- Winner reference --- */
ALTER TABLE dbo.Game
ADD CONSTRAINT FK_Game_WinnerGamePlayer
    FOREIGN KEY (WinnerGamePlayerID) REFERENCES dbo.GamePlayer(GamePlayerID);

/* --- Helpful indexes --- */
CREATE INDEX IX_Move_Game_Turn ON dbo.Move(GameID, TurnNumber);
CREATE INDEX IX_Tile_Game ON dbo.Tile(GameID);
CREATE INDEX IX_Game_Status ON dbo.Game(Status);
GO

/* --- Quick sanity checks --- */
SELECT TOP 0 * FROM dbo.[User];
SELECT TOP 0 * FROM dbo.Game;
SELECT TOP 0 * FROM dbo.GamePlayer;
SELECT TOP 0 * FROM dbo.Card;
SELECT TOP 0 * FROM dbo.Tile;
SELECT TOP 0 * FROM dbo.Move;
