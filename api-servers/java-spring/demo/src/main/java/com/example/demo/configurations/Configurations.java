package com.example.demo.configurations;

import com.example.demo.configurations.templates.RedisTemplate;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.annotation.Bean;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.web.servlet.LocaleResolver;
import org.springframework.web.servlet.config.annotation.WebMvcConfigurer;
import org.springframework.web.servlet.i18n.LocaleChangeInterceptor;
import org.springframework.web.servlet.i18n.SessionLocaleResolver;

import java.util.Collection;
import java.util.Locale;
import java.util.Map;

public class Configurations {

    // CORS Configuration

    @Bean
    public static WebMvcConfigurer corsConfigurer() {

        /*
        return new WebMvcConfigurerAdapter() {

            @Override
            public void addCorsMappings(CorsRegistry registry) {
                registry.addMapping("/products")
                        .allowedOrigins("http://locahost:8080"); // todo: env-var

            }

        }
        */
        return null;
    }

    // Internationalization

    @Bean
    public static LocaleResolver localeResolver() {
        SessionLocaleResolver sessionLocaleResolver =
                new SessionLocaleResolver();
        sessionLocaleResolver.setDefaultLocale(Locale.US);
        return sessionLocaleResolver;
    }

    @Bean
    public static LocaleChangeInterceptor localeChangeInterceptor() {
        LocaleChangeInterceptor localeChangeInterceptor =
                new LocaleChangeInterceptor();
        localeChangeInterceptor.setParamName("language");
        return localeChangeInterceptor;
    }

    // ..

    // * Redis Template

//    @Autowired
    RedisTemplate<String, Object> redis = new RedisTemplate<>();

    Map<Object, Object> dataList =
            redis.opsForHash();//.entries("Redis_code_index_key");

    // JDBC Template

//    @Autowired
    JdbcTemplate jdbcTemplate;

    Collection<Map<String, Object>> rows = null; // jdbc.queryForList("SELECT QUERY");

}