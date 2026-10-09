from typing import List, Optional

from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    # --- LLM Provider ---
    LLM_PROVIDER: str = "groq"  # groq | gemini
    GROQ_API_KEY: Optional[str] = None
    GROQ_LLM_MODEL: str = "openai/gpt-oss-20b"

    # Gemini / Google (dùng khi LLM_PROVIDER=gemini hoặc fallback)
    GOOGLE_API_KEY: Optional[str] = None
    GEMINI_API_KEY: Optional[str] = None
    GEMINI_LLM_MODEL: str = "gemini-3.6-flash"
    LLM_FALLBACK_MODEL: str = "gemini-3.6-flash"

    LLM_TEMPERATURE: float = 0.0
    MAX_CHAT_HISTORY: int = 20
    CORS_ORIGINS: List[str] = ["http://localhost:3000", "http://localhost:5000"]

    # --- Backend ---
    BACKEND_URL: str = "http://api:8080"

    # --- Agent dự báo nhập hàng ---
    # Tỉnh/thành dùng làm mặc định khi người dùng không nói rõ địa điểm
    DEFAULT_LOCATION: str = "Ha Noi"
    # Mức dự phòng an toàn khi đề xuất nhập (20%)
    SAFETY_STOCK_RATIO: float = 0.2
    FORECAST_LOOKBACK_DAYS: int = 30

    # --- API ngoài (thời tiết / chất lượng không khí / lịch nghỉ lễ) ---
    # OpenWeather dùng chung 1 key cho thời tiết (One Call 3.0), AQI và geocoding
    OPENWEATHER_API_KEY: Optional[str] = None
    EXTERNAL_API_TIMEOUT: float = 10.0
    # Cache TTL (giây): thời tiết/AQI đổi chậm, geocoding gần như tĩnh
    EXTERNAL_CACHE_TTL: int = 1800
    GEOCODE_CACHE_TTL: int = 86400

    # --- Google Trends (pytrends): thư viện không chính thức, dễ bị giới hạn ---
    TRENDS_ENABLED: bool = True

    # --- Langfuse tracing (tùy chọn: thiếu key thì tắt trace, không crash) ---
    LANGFUSE_PUBLIC_KEY: Optional[str] = None
    LANGFUSE_SECRET_KEY: Optional[str] = None
    LANGFUSE_BASE_URL: Optional[str] = None 
    LANGFUSE_TRACING_ENVIRONMENT: Optional[str] = None
    LANGFUSE_RELEASE: Optional[str] = None
    LANGFUSE_SAMPLE_RATE: float = 1.0

    model_config = {"env_file": ".env", "extra": "ignore"}

    @property
    def effective_gemini_api_key(self) -> Optional[str]:
        return self.GEMINI_API_KEY or self.GOOGLE_API_KEY

    @property
    def langfuse_enabled(self) -> bool:
        return bool(self.LANGFUSE_PUBLIC_KEY and self.LANGFUSE_SECRET_KEY)

    @property
    def openweather_enabled(self) -> bool:
        return bool(self.OPENWEATHER_API_KEY)

    @property
    def trends_available(self) -> bool:
        return self.TRENDS_ENABLED


settings = Settings()