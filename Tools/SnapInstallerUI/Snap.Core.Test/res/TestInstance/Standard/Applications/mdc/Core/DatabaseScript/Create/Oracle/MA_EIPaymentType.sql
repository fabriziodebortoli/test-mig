CREATE TABLE "MA_EIPAYMENTTYPE" (
    "PAYMENTTYPE" NUMBER (10) NOT NULL,
    "EICODE" VARCHAR2 (8) NOT NULL,
    CONSTRAINT "PK_EIPAYMENTTYPE" PRIMARY KEY
    (
        "PAYMENTTYPE",
        "EICODE"
    )
)
GO