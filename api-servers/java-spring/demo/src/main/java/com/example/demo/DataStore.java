package com.example.demo;

import java.util.HashMap;

public class DataStore {

    private HashMap<String, String> store = new HashMap<String, String>();

    public DataStore() {
        store.put("key", "value");
    }

    public String getFromDB(String word) {
        return store.get(word);
    }

}
