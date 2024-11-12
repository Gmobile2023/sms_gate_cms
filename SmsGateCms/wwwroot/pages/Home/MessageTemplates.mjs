import { ref } from "vue";
import { useClient, useFormatters } from "@servicestack/vue";
import {GetPartners} from "../../mjs/dtos.mjs";
// import { CreateMessage, UpdateMessage } from "dtos.mjs"

export default {
    template: /*html*/`
    <auto-query-grid type="MessageTemplate" :visible-from="{ }">
        
         <template #content-header>Nội dung</template>
        <template #status-header>Trạng thái</template>
        <template #partnerName-header>đối tác</template>
        <template #createdDate-header>Ngày tạo</template>
        <template #modifiedDate-header>Ngày sửa</template>
        <template #createdBy-header>Người tạo</template>
        <template #modifiedBy-header>Người sửa</template>
        <template #id="{ id }">
            <span v-html="id"></span>
        </template>
        <template #content="{ content }">
            <span v-html="content"></span>
        </template>
        <template #status="{ status }">
            <span :class="statusClass(status)">{{ statusMap[status] || status }}</span>
        </template>
        <template #partnerName="{ partnerName }">
            <text-link v-if="partnerName" class="flex items-end" @click.stop="showPartner(partnerId)" :title="partnerName.id">
                <icon class="w-5 h-5 mr-1" type="Partner"></icon>
                <preview-format :value="partnerName.partnerName"></preview-format>
            </text-link>
        </template>
        <template #createdDate="{ createdDate }">
            <span v-html="formatDateTime(createdDate)"></span>
        </template>
         <template #modifiedDate="{ modifiedDate }">
            <span v-html="formatDateTime(modifiedDate)"></span>
        </template>

        <template #createdBy="{ createdBy }">
            <span v-html="createdBy"></span>
        </template>

        <template #modifiedBy="{ modifiedBy }">
            <span v-html="modifiedBy"></span>
        </template>
    </auto-query-grid> 
    <auto-edit-form v-if="partner" type="UpdatePartner" v-model="partner" v-on:done="close" v-on:save="close"></auto-edit-form>
  `,
    setup() {
        const client = useClient();
        const partner = ref();
        const { currency } = useFormatters()
        const formatDateTime = (value) => {
            if (!value) return '';

            const date = new Date(value);

            const timeZoneOffset = 7 * 60 * 60000; // Asia/Ho_Chi_Minh có UTC+7
            const localTime = new Date(date.getTime() + timeZoneOffset);

            return new Intl.DateTimeFormat('vi-VN', {dateStyle: 'medium', timeStyle: 'short'}).format(localTime)
        };

        const formatDateTimeLocal = (value) => {
            if (!value) return '';

            const date = new Date(value);

            const localTime = new Date(date.getTime());

            return new Intl.DateTimeFormat('vi-VN', {dateStyle: 'medium', timeStyle: 'short'}).format(localTime)
        };

        const statusMap = {
            "Initial": "Khởi tạo",
            "Active": "Đã duyệt",
            "Cancel": "Hủy",
            "Locked": "Khóa"
        };

        const statusClass = (status) => {
            return {
                "text-gray-500": status === "Initial",
                "text-blue-500": status === "Active",
                "text-green-500": status === "Cancel",
                "text-red-500": status === "Locked"
            };
        };
        async function showPartner(id) {
            const api = await client.api(new GetPartners({ id }))
            if (api.succeeded) {
                console.log(api);
                partner.value = api.response.results[0]
            }
        }

        const close = () => {
            importFile.value = null,
            partner.value = null
        }

        return {
            partner,
            formatDateTime,
            statusMap,
            close,
            formatDateTimeLocal,
            showPartner,
            statusClass,
            currency
        }
    }
}