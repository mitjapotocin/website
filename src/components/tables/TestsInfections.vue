<template>
  <b-table
    responsive
    bordered
    outlined
    no-border-collapse
    striped
    hover
    sort-by="date"
    :sort-desc="true"
    :sticky-header="tableHeight"
    :items="csvdata"
    :fields="fields"
  >
    <template v-slot:head()="scope">
      <div class="text-nowrap">{{ scope.label }}</div>
    </template>
    <template v-slot:cell(date)="data">
      <div class="text-nowrap">{{ data.item.date | formatDate('dd. MMMM') }}</div>
    </template>
    <template v-slot:empty="scope">
      <h4>{{ scope.emptyText }} ni podatka</h4>
    </template>
  </b-table>
</template>

<script>
export default {
  props: ["csvdata"],
  data() {
    return {
      // at least we don't have double scroll on mobile
      tableHeight: `${window.innerHeight - 230}px`,
      fields: [
        {
          key: "date",
          headerTitle: "Datum",
          label: "Datum",
          sortable: true,
          stickyColumn: true,
          variant: 'grey'
        },
        {
          key: "tests.performed",
          label: "Opravljenih testov"
        },
        {
          key: "tests.performed.todate",
          label: "Opravljenih testov - Skupno"
        },
        {
          key: "tests.positive",
          label: "Potrjene okužbe"
        },
        {
          key: "tests.positive.todate",
          label: "Potrjene okužbe - Skupno"
        },
        {
          key: "state.in_hospital",
          label: "Hospitalizirani"
        },
        {
          key: "state.icu",
          label: "V intenzivnih negi"
        },
        {
          key: "state.critical",
          label: "V kritičnem stanju"
        },
        {
          key: "state.deceased.todate",
          label: "Umrli"
        },
        {
          key: "state.out_of_hospital.todate",
          label: "Odpuščeni iz bolnišnice"
        },
        {
          key: "state.recovered.todate",
          label: "Ozdravljeni"
        }
      ]
    };
  }
};
</script>

<style>
</style>