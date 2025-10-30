USE calculadora

CREATE TABLE calculos (
    expresion VARCHAR(255) NOT NULL,
    resultado FLOAT NOT NULL,
    fecha DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
)