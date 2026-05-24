// ============================================
// EXTRA INTERIORS & ACTIVITIES - MAX LEVEL
// ============================================

// Additional IPL loading for more accessible buildings
setTimeout(() => {
    // Tuner DLC interiors
    try { mp.game.streaming.requestIpl("dvs_autoshop_dl_intwaremed"); } catch(e) {}
    
    // Stadium
    try { mp.game.streaming.requestIpl("sp1_10_real_interior"); } catch(e) {}
    
    // Additional Apartments (Eclipse Towers)
    try {
        mp.game.streaming.requestIpl("apa_v_mp_h_01_a");
        mp.game.streaming.requestIpl("apa_v_mp_h_01_b");
        mp.game.streaming.requestIpl("apa_v_mp_h_01_c");
        mp.game.streaming.requestIpl("apa_v_mp_h_02_a");
        mp.game.streaming.requestIpl("apa_v_mp_h_02_b");
        mp.game.streaming.requestIpl("apa_v_mp_h_02_c");
        mp.game.streaming.requestIpl("apa_v_mp_h_03_a");
        mp.game.streaming.requestIpl("apa_v_mp_h_03_b");
        mp.game.streaming.requestIpl("apa_v_mp_h_03_c");
        mp.game.streaming.requestIpl("apa_v_mp_h_04_a");
        mp.game.streaming.requestIpl("apa_v_mp_h_04_b");
        mp.game.streaming.requestIpl("apa_v_mp_h_04_c");
        mp.game.streaming.requestIpl("apa_v_mp_h_05_a");
        mp.game.streaming.requestIpl("apa_v_mp_h_05_c");
        mp.game.streaming.requestIpl("apa_v_mp_h_06_a");
        mp.game.streaming.requestIpl("apa_v_mp_h_06_b");
        mp.game.streaming.requestIpl("apa_v_mp_h_06_c");
        mp.game.streaming.requestIpl("apa_v_mp_h_07_a");
        mp.game.streaming.requestIpl("apa_v_mp_h_07_b");
        mp.game.streaming.requestIpl("apa_v_mp_h_07_c");
        mp.game.streaming.requestIpl("apa_v_mp_h_08_a");
        mp.game.streaming.requestIpl("apa_v_mp_h_08_b");
        mp.game.streaming.requestIpl("apa_v_mp_h_08_c");
    } catch(e) {}

    // Stilt Houses
    try {
        mp.game.streaming.requestIpl("apa_stilt_ch2_06a_ext");
        mp.game.streaming.requestIpl("apa_stilt_ch2_06b_ext");
        mp.game.streaming.requestIpl("apa_stilt_ch2_06c_ext");
        mp.game.streaming.requestIpl("apa_stilt_ch2_09a_ext");
        mp.game.streaming.requestIpl("apa_stilt_ch2_09b_ext");
        mp.game.streaming.requestIpl("apa_stilt_ch2_09c_ext");
    } catch(e) {}

    // More CEO Offices
    try {
        mp.game.streaming.requestIpl("ex_dt1_11_office_01b");
        mp.game.streaming.requestIpl("ex_dt1_11_office_01c");
        mp.game.streaming.requestIpl("ex_dt1_11_office_02a");
        mp.game.streaming.requestIpl("ex_dt1_11_office_02b");
        mp.game.streaming.requestIpl("ex_dt1_11_office_02c");
        mp.game.streaming.requestIpl("ex_dt1_11_office_03a");
        mp.game.streaming.requestIpl("ex_dt1_11_office_03b");
        mp.game.streaming.requestIpl("ex_dt1_11_office_03c");
        mp.game.streaming.requestIpl("ex_sm_13_office_01a");
        mp.game.streaming.requestIpl("ex_sm_13_office_01b");
        mp.game.streaming.requestIpl("ex_sm_13_office_01c");
        mp.game.streaming.requestIpl("ex_sm_13_office_02b");
        mp.game.streaming.requestIpl("ex_sm_13_office_02c");
        mp.game.streaming.requestIpl("ex_sm_13_office_03a");
        mp.game.streaming.requestIpl("ex_sm_13_office_03b");
        mp.game.streaming.requestIpl("ex_sm_13_office_03c");
    } catch(e) {}

    // Bunker interiors
    try {
        mp.game.streaming.requestIpl("gr_case0_bunkerclosed");
        mp.game.streaming.requestIpl("gr_case1_bunkerclosed");
        mp.game.streaming.requestIpl("gr_case2_bunkerclosed");
        mp.game.streaming.requestIpl("gr_case3_bunkerclosed");
        mp.game.streaming.requestIpl("gr_case4_bunkerclosed");
        mp.game.streaming.requestIpl("gr_case5_bunkerclosed");
        mp.game.streaming.requestIpl("gr_case7_bunkerclosed");
        mp.game.streaming.requestIpl("gr_case9_bunkerclosed");
        mp.game.streaming.requestIpl("gr_case10_bunkerclosed");
        mp.game.streaming.requestIpl("gr_case11_bunkerclosed");
    } catch(e) {}

    // Motorcycle Clubhouses
    try {
        mp.game.streaming.requestIpl("bkr_bi_hw1_13_int");
        mp.game.streaming.requestIpl("bkr_bi_id1_20_interior_v_dvl_milo_");
        mp.game.streaming.requestIpl("bkr_bi_id1_21_grd_milo_");
        mp.game.streaming.requestIpl("bkr_bi_id1_23_interior_v_dvl_milo_");
    } catch(e) {}

    // Warehouses
    try {
        mp.game.streaming.requestIpl("ex_exec_warehouse_placement_interior_0_int_warehouse_s_dlc_milo");
        mp.game.streaming.requestIpl("ex_exec_warehouse_placement_interior_1_int_warehouse_m_dlc_milo");
        mp.game.streaming.requestIpl("ex_exec_warehouse_placement_interior_2_int_warehouse_l_dlc_milo");
    } catch(e) {}

    // Heist Interiors
    try {
        mp.game.streaming.requestIpl("hei_fi_fh1_milo_");
        mp.game.streaming.requestIpl("hei_fi_fh2_milo_");
        mp.game.streaming.requestIpl("hei_hc_bio_milo_");
        mp.game.streaming.requestIpl("hei_bi_hw1_13_door");
    } catch(e) {}

}, 2000);

// Enhanced world features - More props and activities
setTimeout(() => {
    // Enable additional world features
    mp.game.misc.setStuntJumpsCanTrigger(true);
    mp.game.misc.setRandomEventFlag(true);
    
    // Better vehicle density for more alive world
    mp.game.streaming.setVehiclePopulationBudget(3);
    mp.game.streaming.setPedPopulationBudget(3);
}, 3000);
